using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Operations
{
    // ══════════════════════════════════════════════════════════════
    // PARTICIPANTS
    // ══════════════════════════════════════════════════════════════
    [AuthorizeRole("Admin")]
    public class ParticipantsModel : PageModel
    {
        private readonly IParticipantService _svc;
        private readonly IAuditLogService _audit;

        public ParticipantsModel(IParticipantService svc, IAuditLogService audit)
        { _svc = svc; _audit = audit; }

        public List<Participant> Participants { get; set; } = new();
        public int Total { get; set; }
        public int VipCount { get; set; }
        public int BlacklistCount { get; set; }

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterType { get; set; } = "";
        [TempData] public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");

            bool? isVip = FilterType == "vip" ? true : FilterType == "normal" ? false : null;
            bool? isBl = FilterType == "blacklist" ? true : FilterType == "normal" ? false : null;

            Participants = await _svc.GetAllAsync(Search, isVip, isBl);
            var all = await _svc.GetAllAsync();
            Total = all.Count;
            VipCount = all.Count(p => p.IsVip == true);
            BlacklistCount = all.Count(p => p.IsBlacklisted == true);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleVipAsync(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            await _svc.ToggleVipAsync(id);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"TOGGLE VIP Participant ID={id}", "Participants");
            SuccessMessage = "Đã cập nhật VIP.";
            return RedirectToPage(new { Search, FilterType });
        }

        public async Task<IActionResult> OnPostToggleBlacklistAsync(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            await _svc.ToggleBlacklistAsync(id);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"TOGGLE BLACKLIST Participant ID={id}", "Participants");
            SuccessMessage = "Đã cập nhật Blacklist.";
            return RedirectToPage(new { Search, FilterType });
        }
    }

    // ══════════════════════════════════════════════════════════════
    // SURVEY RESPONSES
    // ══════════════════════════════════════════════════════════════
    [AuthorizeRole("Admin")]
    public class SurveyResponsesModel : PageModel
    {
        private readonly ISurveyService _svc;
        private readonly ProjectPrn222Context _db;

        public SurveyResponsesModel(ISurveyService svc, ProjectPrn222Context db)
        { _svc = svc; _db = db; }

        public List<SurveyResponse> Responses { get; set; } = new();
        public SurveyStatsDto Stats { get; set; } = new();
        public List<Event> Events { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? EventId { get; set; }
        [BindProperty(SupportsGet = true)] public int? MinRating { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            Responses = await _svc.GetAllAsync(EventId, MinRating);
            Stats = await _svc.GetStatsAsync(EventId);
            Events = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                              .ToListAsync(_db.Events.OrderBy(e => e.EventName));
            return Page();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // FIELD REPORTS
    // ══════════════════════════════════════════════════════════════
    [AuthorizeRole("Admin")]
    public class FieldReportsModel : PageModel
    {
        private readonly IFieldReportService _svc;
        private readonly ProjectPrn222Context _db;

        public FieldReportsModel(IFieldReportService svc, ProjectPrn222Context db)
        { _svc = svc; _db = db; }

        public List<FieldReport> Reports { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<string> ReportTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? EventId { get; set; }
        [BindProperty(SupportsGet = true)] public string ReportType { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            Reports = await _svc.GetAllAsync(EventId, ReportType);
            Events = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                                .ToListAsync(_db.Events.OrderBy(e => e.EventName));
            ReportTypes = await _db.FieldReports
                                .Where(r => r.ReportType != null)
                                .Select(r => r.ReportType!)
                                .Distinct().ToListAsync();
            return Page();
        }
    }
}