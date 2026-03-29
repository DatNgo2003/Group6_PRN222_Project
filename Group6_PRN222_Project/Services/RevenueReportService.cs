using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── DTOs ─────────────────────────────────────────────────────
    public class RevenueReportDto
    {
        // ── Tổng quan tài chính ──────────────────────────
        public decimal TotalAllocated { get; set; }  // tổng ngân sách được duyệt
        public decimal TotalSpent { get; set; }  // tổng đã chi
        public decimal TotalProfit { get; set; }  // lợi nhuận (từ Budgets)
        public decimal TotalContractValue { get; set; }  // tổng giá trị hợp đồng
        public decimal TotalTicketRevenue { get; set; }  // doanh thu từ vé paid
        public decimal TotalAdSpend { get; set; }  // tổng chi marketing
        public int TotalEvents { get; set; }

        // ── Chart: lợi nhuận theo sự kiện ────────────────
        public List<EventFinanceDto> EventFinances { get; set; } = new();

        // ── Chart: ngân sách theo tháng (StartDate) ──────
        public List<MonthlyBudgetDto> MonthlyBudgets { get; set; } = new();

        // ── Trạng thái duyệt ngân sách ───────────────────
        public int BudgetApproved { get; set; }
        public int BudgetPending { get; set; }
        public int BudgetRejected { get; set; }
    }

    public class EventFinanceDto
    {
        public string EventName { get; set; } = "";
        public string? Status { get; set; }
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Profit { get; set; }
        public decimal ContractValue { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal AdSpend { get; set; }
        public string ApprovalStatus { get; set; } = "";
        // Tỷ lệ đã chi / ngân sách
        public double SpentRate => Allocated == 0 ? 0
            : Math.Round((double)(Spent / Allocated) * 100, 1);
    }

    public class MonthlyBudgetDto
    {
        public string Label { get; set; } = "";
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Profit { get; set; }
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
            // ── Load events (có filter) ───────────────────
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

            var campaigns = await _db.MarketingCampaigns
                .Where(m => m.EventId != null && eventIds.Contains(m.EventId.Value))
                .AsNoTracking().ToListAsync();

            // ── Summary ───────────────────────────────────
            var dto = new RevenueReportDto
            {
                TotalEvents = events.Count,
                TotalAllocated = budgets.Sum(b => b.TotalAllocated ?? 0),
                TotalSpent = budgets.Sum(b => b.SpentAmount ?? 0),
                TotalProfit = tickets.Sum(t => t.Price ?? 0)
                                   - budgets.Sum(b => b.SpentAmount ?? 0),
                TotalContractValue = contracts.Sum(c => c.ContractValue ?? 0),
                TotalTicketRevenue = tickets.Sum(t => t.Price ?? 0),
                TotalAdSpend = campaigns.Sum(m => m.AdSpend ?? 0),
                BudgetApproved = budgets.Count(b => b.ApprovalStatus == "Approved"),
                BudgetPending = budgets.Count(b => b.ApprovalStatus == "Pending"),
                BudgetRejected = budgets.Count(b => b.ApprovalStatus == "Rejected"),
            };

            // ── Per-event finance ─────────────────────────
            dto.EventFinances = events.Select(ev =>
            {
                var b = budgets.FirstOrDefault(x => x.EventId == ev.EventId);
                return new EventFinanceDto
                {
                    EventName = ev.EventName ?? "",
                    Status = ev.Status,
                    Allocated = b?.TotalAllocated ?? 0,
                    Spent = b?.SpentAmount ?? 0,
                    Profit = tickets
                        .Where(t => t.EventId == ev.EventId)
                        .Sum(t => t.Price ?? 0)
                        - (b?.SpentAmount ?? 0),
                    ApprovalStatus = b?.ApprovalStatus ?? "—",
                    ContractValue = contracts
                        .Where(c => c.EventId == ev.EventId)
                        .Sum(c => c.ContractValue ?? 0),
                    TicketRevenue = tickets
                        .Where(t => t.EventId == ev.EventId)
                        .Sum(t => t.Price ?? 0),
                    AdSpend = campaigns
                        .Where(m => m.EventId == ev.EventId)
                        .Sum(m => m.AdSpend ?? 0),
                };
            })
            .OrderByDescending(e => e.Allocated)
            .ToList();

            // ── Monthly breakdown theo StartDate ──────────
            dto.MonthlyBudgets = events
                .Where(e => e.StartDate.HasValue)
                .GroupBy(e => new {
                    e.StartDate!.Value.Year,
                    e.StartDate!.Value.Month
                })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var ids = g.Select(e => e.EventId).ToList();
                    var bs = budgets.Where(b => b.EventId.HasValue && ids.Contains(b.EventId.Value)).ToList();
                    return new MonthlyBudgetDto
                    {
                        Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Allocated = bs.Sum(b => b.TotalAllocated ?? 0),
                        Spent = bs.Sum(b => b.SpentAmount ?? 0),
                        Profit = bs.Sum(b => b.Profit ?? 0),
                    };
                })
                .ToList();

            return dto;
        }
    }
}