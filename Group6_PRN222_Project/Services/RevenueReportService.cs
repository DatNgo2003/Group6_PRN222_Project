using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── DTOs ─────────────────────────────────────────────────────
    public class RevenueReportDto
    {
        public decimal TotalAllocated { get; set; }  // tổng ngân sách được duyệt
        public decimal TotalProfit { get; set; }  // vé - marketing - logistics
        public decimal TotalContractValue { get; set; }  // tổng giá trị hợp đồng
        public decimal TotalTicketRevenue { get; set; }  // doanh thu từ vé paid
        public decimal TotalMarketing { get; set; }  // chi marketing (FieldReports type=Marketing)
        public decimal TotalLogistics { get; set; }  // chi logistics (FieldReports type=Logistics)
        public int TotalEvents { get; set; }

        public List<EventFinanceDto> EventFinances { get; set; } = new();

        public int BudgetApproved { get; set; }
        public int BudgetPending { get; set; }
        public int BudgetRejected { get; set; }
    }

    public class EventFinanceDto
    {
        public string EventName { get; set; } = "";
        public string? Status { get; set; }
        public decimal Allocated { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal Marketing { get; set; }  // FieldReports[Marketing]
        public decimal Logistics { get; set; }  // FieldReports[Logistics]
        public decimal Profit { get; set; }  // = TicketRevenue - Marketing - Logistics
        public decimal ContractValue { get; set; }
        public string ApprovalStatus { get; set; } = "";
    }

    // ─── Interface ────────────────────────────────────────────────
    public interface IRevenueReportService
    {
        Task<RevenueReportDto> GetReportAsync(DateTime? from, DateTime? to, int? eventId = null);
        Task<List<(int Id, string Name)>> GetEventListAsync();
    }

    // ─── Implementation ───────────────────────────────────────────
    public class RevenueReportService : IRevenueReportService
    {
        private readonly ProjectPrn222Context _db;
        public RevenueReportService(ProjectPrn222Context db) => _db = db;

        public async Task<List<(int Id, string Name)>> GetEventListAsync()
        {
            var list = await _db.Events
                .OrderByDescending(e => e.StartDate)
                .Select(e => new { e.EventId, Name = e.EventName ?? "" })
                .ToListAsync();
            return list.Select(e => (e.EventId, e.Name)).ToList();
        }

        public async Task<RevenueReportDto> GetReportAsync(
            DateTime? from, DateTime? to, int? eventId = null)
        {
            // ── Load events ───────────────────────────────
            var evQuery = _db.Events.AsQueryable();
            if (eventId.HasValue)
                evQuery = evQuery.Where(e => e.EventId == eventId.Value);
            if (from.HasValue)
                evQuery = evQuery.Where(e => e.StartDate >= from.Value);
            if (to.HasValue)
                evQuery = evQuery.Where(e => e.StartDate <= to.Value.AddDays(1));

            var events = await evQuery.AsNoTracking().ToListAsync();
            var eventIds = events.Select(e => e.EventId).ToList();

            // ── Load related data ─────────────────────────
            var budgets = await _db.Budgets
                .Where(b => b.EventId != null && eventIds.Contains(b.EventId.Value))
                .AsNoTracking().ToListAsync();

            var contracts = await _db.Contracts
                .Where(c => c.EventId != null && eventIds.Contains(c.EventId.Value))
                .AsNoTracking().ToListAsync();

            var tickets = await _db.Tickets
                .Where(t => t.EventId != null && eventIds.Contains(t.EventId.Value)
                         && t.PaymentStatus != null
                         && t.PaymentStatus.ToLower() == "paid")
                .AsNoTracking().ToListAsync();


            // Chỉ lấy FieldReports có ReportType là Marketing hoặc Logistics
            var fieldReports = await _db.FieldReports
                .Where(f => f.EventId != null && eventIds.Contains(f.EventId.Value)
                         && (f.ReportType == "Marketing" || f.ReportType == "EquipmentRentalRequest"))
                .AsNoTracking().ToListAsync();

            // ── Tính tổng ─────────────────────────────────
            var totalTicket = tickets.Sum(t => t.Price ?? 0);

            var totalMarketing = fieldReports
                .Where(f => f.ReportType == "Marketing")
                .Sum(f => (decimal)(f.EstimatePrice ?? 0));
            var totalLogistics = fieldReports
                .Where(f => f.ReportType == "EquipmentRentalRequest")
                .Sum(f => (decimal)(f.EstimatePrice ?? 0));

            // ── Summary ───────────────────────────────────
            var dto = new RevenueReportDto
            {
                TotalEvents = events.Count,
                TotalAllocated = budgets.Sum(b => b.TotalAllocated ?? 0),
                TotalTicketRevenue = totalTicket,

                TotalMarketing = totalMarketing,
                TotalLogistics = totalLogistics,
                TotalProfit = totalTicket - totalMarketing - totalLogistics,
                TotalContractValue = contracts.Sum(c => c.ContractValue ?? 0),
                BudgetApproved = budgets.Count(b => b.ApprovalStatus == "Approved"),
                BudgetPending = budgets.Count(b => b.ApprovalStatus == "Pending"),
                BudgetRejected = budgets.Count(b => b.ApprovalStatus == "Rejected"),
            };

            // ── Per-event finance ─────────────────────────
            dto.EventFinances = events.Select(ev =>
            {
                var b = budgets.FirstOrDefault(x => x.EventId == ev.EventId);
                var evTicket = tickets.Where(t => t.EventId == ev.EventId).Sum(t => t.Price ?? 0);

                var evMarketing = fieldReports
                    .Where(f => f.EventId == ev.EventId && f.ReportType == "Marketing")
                    .Sum(f => (decimal)(f.EstimatePrice ?? 0));
                var evLogistics = fieldReports
                    .Where(f => f.EventId == ev.EventId && f.ReportType == "EquipmentRentalRequest")
                    .Sum(f => (decimal)(f.EstimatePrice ?? 0));

                return new EventFinanceDto
                {
                    EventName = ev.EventName ?? "",
                    Status = ev.Status,
                    Allocated = b?.TotalAllocated ?? 0,
                    ApprovalStatus = b?.ApprovalStatus ?? "—",
                    ContractValue = contracts.Where(c => c.EventId == ev.EventId).Sum(c => c.ContractValue ?? 0),
                    TicketRevenue = evTicket,

                    Marketing = evMarketing,
                    Logistics = evLogistics,
                    Profit = evTicket - evMarketing - evLogistics,
                };
            })
            .OrderByDescending(e => e.Allocated)
            .ToList();

            return dto;
        }
    }
}
