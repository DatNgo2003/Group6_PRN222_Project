using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── DTOs ─────────────────────────────────────────────────────
    public class DepartmentKpiDto
    {
        public int     DepartmentId   { get; set; }
        public string  DepartmentName { get; set; } = "";
        public int     TotalUsers     { get; set; }
        public int     TotalEvents    { get; set; }
        public int     TotalTickets   { get; set; }
        public int     PaidTickets    { get; set; }
        public decimal TotalRevenue   { get; set; }

        // Tỷ lệ chuyển đổi ticket paid / tổng ticket (%)
        public double ConversionRate => TotalTickets == 0 ? 0
            : Math.Round((double)PaidTickets / TotalTickets * 100, 1);
    }

    public class DepartmentKpiSummary
    {
        public List<DepartmentKpiDto> Departments { get; set; } = new();
        public int     TotalUsers    { get; set; }
        public int     TotalEvents   { get; set; }
        public decimal TotalRevenue  { get; set; }
    }

    // ─── Interface ────────────────────────────────────────────────
    public interface IDepartmentKpiService
    {
        Task<DepartmentKpiSummary> GetKpisAsync(DateTime? from, DateTime? to);
    }

    // ─── Implementation ───────────────────────────────────────────
    public class DepartmentKpiService : IDepartmentKpiService
    {
        private readonly ProjectPrn222Context _db;

        public DepartmentKpiService(ProjectPrn222Context db)
        {
            _db = db;
        }

        public async Task<DepartmentKpiSummary> GetKpisAsync(DateTime? from, DateTime? to)
        {
            var departments = await _db.Departments.ToListAsync();

            var usersByDept = await _db.Users
                .Where(u => u.DepartmentId != null)
                .GroupBy(u => u.DepartmentId!.Value)
                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeptId, x => x.Count);

            // Query events có filter theo StartDate
            var eventQuery = _db.Events.AsQueryable();
            if (from.HasValue)
                eventQuery = eventQuery.Where(e => e.StartDate >= from.Value);
            if (to.HasValue)
                eventQuery = eventQuery.Where(e => e.StartDate <= to.Value.AddDays(1));

            // Lấy events kèm tickets
            var events = await eventQuery
                .Include(e => e.Tickets)
                .ToListAsync();

            // Group events theo DepartmentId của Organizer phụ trách
            // (giả sử Event có OrganizerId → User.DepartmentId)
            var organizers = await _db.Users
                .Where(u => u.DepartmentId != null)
                .Select(u => new { u.UserId, u.DepartmentId })
                .ToListAsync();
            var orgDeptMap = organizers.ToDictionary(o => o.UserId, o => o.DepartmentId!.Value);

            var kpis = departments.Select(dept =>
            {
                // Events thuộc department (qua organizer)
                var deptEvents = events
                    .Where(e => e.OrganizerId.HasValue
                             && orgDeptMap.TryGetValue(e.OrganizerId.Value, out var dId)
                             && dId == dept.DepartmentId)
                    .ToList();

                var allTickets  = deptEvents.SelectMany(e => e.Tickets).ToList();
                var paidTickets = allTickets.Where(t => t.PaymentStatus == "Paid").ToList();

                return new DepartmentKpiDto
                {
                    DepartmentId   = dept.DepartmentId,
                    DepartmentName = dept.DepartmentName ?? "—",
                    TotalUsers     = usersByDept.GetValueOrDefault(dept.DepartmentId, 0),
                    TotalEvents    = deptEvents.Count,
                    TotalTickets   = allTickets.Count,
                    PaidTickets    = paidTickets.Count,
                    TotalRevenue   = paidTickets.Count  // placeholder — không có cột Price
                };
            }).ToList();

            return new DepartmentKpiSummary
            {
                Departments  = kpis.OrderByDescending(d => d.TotalRevenue).ToList(),
                TotalUsers   = usersByDept.Values.Sum(),
                TotalEvents  = events.Count,
                TotalRevenue = kpis.Sum(k => k.TotalRevenue)
            };
        }
    }
}
