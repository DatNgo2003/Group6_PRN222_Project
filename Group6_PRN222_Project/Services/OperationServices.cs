using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ══════════════════════════════════════════════════════════════
    // PARTICIPANT SERVICE
    // ══════════════════════════════════════════════════════════════
    public interface IParticipantService
    {
        Task<List<Participant>> GetAllAsync(string? search = null, bool? isVip = null, bool? isBlacklisted = null);
        Task<Participant?>      GetByIdAsync(int id);
        Task                   ToggleBlacklistAsync(int id);
        Task                   ToggleVipAsync(int id);
    }

    public class ParticipantService : IParticipantService
    {
        private readonly ProjectPrn222Context _db;
        public ParticipantService(ProjectPrn222Context db) => _db = db;

        public Task<List<Participant>> GetAllAsync(string? search = null, bool? isVip = null, bool? isBlacklisted = null)
        {
            var q = _db.Participants.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => (p.FullName != null && p.FullName.Contains(search))
                               || (p.Email    != null && p.Email.Contains(search))
                               || (p.Phone    != null && p.Phone.Contains(search)));
            if (isVip.HasValue)
                q = q.Where(p => p.IsVip == isVip.Value);
            if (isBlacklisted.HasValue)
                q = q.Where(p => p.IsBlacklisted == isBlacklisted.Value);
            return q.OrderBy(p => p.ParticipantId).ToListAsync();
        }

        public Task<Participant?> GetByIdAsync(int id)
            => _db.Participants.FirstOrDefaultAsync(p => p.ParticipantId == id);

        public async Task ToggleBlacklistAsync(int id)
        {
            var p = await _db.Participants.FindAsync(id);
            if (p == null) return;
            p.IsBlacklisted = !(p.IsBlacklisted ?? false);
            await _db.SaveChangesAsync();
        }

        public async Task ToggleVipAsync(int id)
        {
            var p = await _db.Participants.FindAsync(id);
            if (p == null) return;
            p.IsVip = !(p.IsVip ?? false);
            await _db.SaveChangesAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // SURVEY RESPONSE SERVICE
    // ══════════════════════════════════════════════════════════════
    public class SurveyStatsDto
    {
        public double AvgRating    { get; set; }
        public int    TotalCount   { get; set; }
        public int    Rating5      { get; set; }
        public int    Rating4      { get; set; }
        public int    Rating3      { get; set; }
        public int    Rating2      { get; set; }
        public int    Rating1      { get; set; }
    }

    public interface ISurveyService
    {
        Task<List<SurveyResponse>> GetAllAsync(int? eventId = null, int? minRating = null);
        Task<SurveyStatsDto>       GetStatsAsync(int? eventId = null);
    }

    public class SurveyService : ISurveyService
    {
        private readonly ProjectPrn222Context _db;
        public SurveyService(ProjectPrn222Context db) => _db = db;

        public Task<List<SurveyResponse>> GetAllAsync(int? eventId = null, int? minRating = null)
        {
            var q = _db.SurveyResponses
                       .Include(s => s.Event)
                       .Include(s => s.Participant)
                       .AsQueryable();
            if (eventId.HasValue)  q = q.Where(s => s.EventId == eventId);
            if (minRating.HasValue) q = q.Where(s => s.Rating >= minRating);
            return q.OrderByDescending(s => s.ResponseId).ToListAsync();
        }

        public async Task<SurveyStatsDto> GetStatsAsync(int? eventId = null)
        {
            var q = _db.SurveyResponses.AsQueryable();
            if (eventId.HasValue) q = q.Where(s => s.EventId == eventId);
            var list = await q.ToListAsync();
            return new SurveyStatsDto
            {
                TotalCount = list.Count,
                AvgRating  = list.Any() ? Math.Round(list.Average(s => s.Rating ?? 0), 1) : 0,
                Rating5    = list.Count(s => s.Rating == 5),
                Rating4    = list.Count(s => s.Rating == 4),
                Rating3    = list.Count(s => s.Rating == 3),
                Rating2    = list.Count(s => s.Rating == 2),
                Rating1    = list.Count(s => s.Rating == 1),
            };
        }
    }

    // ══════════════════════════════════════════════════════════════
    // FIELD REPORT SERVICE
    // ══════════════════════════════════════════════════════════════
    public interface IFieldReportService
    {
        Task<List<FieldReport>> GetAllAsync(int? eventId = null, string? reportType = null);
    }

    public class FieldReportService : IFieldReportService
    {
        private readonly ProjectPrn222Context _db;
        public FieldReportService(ProjectPrn222Context db) => _db = db;

        public Task<List<FieldReport>> GetAllAsync(int? eventId = null, string? reportType = null)
        {
            var q = _db.FieldReports
                       .Include(r => r.Event)
                       .Include(r => r.Staff)
                       .AsQueryable();
            if (eventId.HasValue) q = q.Where(r => r.EventId == eventId);
            if (!string.IsNullOrWhiteSpace(reportType) && reportType != "All")
                q = q.Where(r => r.ReportType == reportType);
            return q.OrderByDescending(r => r.ReportTime).ToListAsync();
        }
    }
}
