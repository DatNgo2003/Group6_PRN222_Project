using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── DTOs ─────────────────────────────────────────────────────
    public class RevenueReportDto
    {
        public int TotalTickets        { get; set; }
        public int PaidTickets         { get; set; }
        public int UnpaidTickets       { get; set; }
        public decimal TotalRevenue    { get; set; }

        public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
        public List<EventRevenueDto>   TopEvents        { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public int     Year    { get; set; }
        public int     Month   { get; set; }
        public string  Label   => $"{Month:D2}/{Year}";
        public int     Tickets { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EventRevenueDto
    {
        public int     EventId    { get; set; }
        public string  EventName  { get; set; } = "";
        public int     PaidCount  { get; set; }
        public decimal Revenue    { get; set; }
    }

    // ─── Interface ────────────────────────────────────────────────
    public interface IRevenueReportService
    {
        Task<RevenueReportDto> GetReportAsync(DateTime? from, DateTime? to);
    }

    // ─── Implementation ───────────────────────────────────────────
    public class RevenueReportService : IRevenueReportService
    {
        private readonly ProjectPrn222Context _db;

        public RevenueReportService(ProjectPrn222Context db)
        {
            _db = db;
        }

        public async Task<RevenueReportDto> GetReportAsync(DateTime? from, DateTime? to)
        {
            var query = _db.Tickets.AsQueryable();

            // Tickets không có CreatedAt → filter qua Event.StartDate
            if (from.HasValue)
                query = query.Where(t => t.Event != null && t.Event.StartDate >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.Event != null && t.Event.StartDate <= to.Value.AddDays(1));

            var tickets = await query
                .Include(t => t.Event)
                .ToListAsync();

            var paid   = tickets.Where(t => t.PaymentStatus == "Paid").ToList();
            var unpaid = tickets.Where(t => t.PaymentStatus != "Paid").ToList();

            // Monthly breakdown — group theo năm+tháng của Event.StartDate
            var monthly = tickets
                .Where(t => t.Event?.StartDate.HasValue == true)
                .GroupBy(t => new { t.Event!.StartDate!.Value.Year, t.Event.StartDate!.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Year    = g.Key.Year,
                    Month   = g.Key.Month,
                    Tickets = g.Count(t => t.PaymentStatus == "Paid"),
                    // Tickets không có Price → dùng số vé paid làm đơn vị đo
                    Revenue = g.Count(t => t.PaymentStatus == "Paid")
                })
                .ToList();

            // Top events theo số vé paid (không có Price)
            var topEvents = tickets
                .Where(t => t.PaymentStatus == "Paid" && t.Event != null)
                .GroupBy(t => new { t.EventId, t.Event!.EventName })
                .Select(g => new EventRevenueDto
                {
                    EventId   = g.Key.EventId ?? 0,
                    EventName = g.Key.EventName ?? "—",
                    PaidCount = g.Count(),
                    Revenue   = g.Count()   // placeholder — không có cột Price
                })
                .OrderByDescending(e => e.PaidCount)
                .Take(10)
                .ToList();

            return new RevenueReportDto
            {
                TotalTickets   = tickets.Count,
                PaidTickets    = paid.Count,
                UnpaidTickets  = unpaid.Count,
                TotalRevenue   = paid.Count,   // placeholder — không có cột Price
                MonthlyBreakdown = monthly,
                TopEvents        = topEvents
            };
        }
    }
}
