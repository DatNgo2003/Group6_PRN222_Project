using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;
using AppTask = System.Threading.Tasks.Task;

namespace Project_PRN222.Pages.StaffMKT.Campaigns
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService _audit;

        public CreateModel(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        public List<Event> Events { get; set; } = new();
        [BindProperty] public int EventId { get; set; }
        [BindProperty] public string AdContent { get; set; } = "";
        [BindProperty] public decimal AdSpend { get; set; }
        [BindProperty] public decimal? EngagementRate { get; set; }
        [BindProperty] public string Status { get; set; } = "Running";

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffMKT);
                HttpContext.Session.SetString("FullName", "Dev Marketing");
                HttpContext.Session.SetString("UserName", "marketing");
            }
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffMKT)
                return RedirectToPage("/Auth/Login");
#endif

            await LoadEventsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffMKT)
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            AdContent = AdContent?.Trim() ?? "";

            if (EventId <= 0)
                ModelState.AddModelError(nameof(EventId), "Vui lòng chọn sự kiện.");
            if (string.IsNullOrWhiteSpace(AdContent))
                ModelState.AddModelError(nameof(AdContent), "Nội dung quảng cáo không được để trống.");
            if (AdSpend < 0)
                ModelState.AddModelError(nameof(AdSpend), "Ad spend không hợp lệ.");
            if (EngagementRate.HasValue && (EngagementRate.Value < 0 || EngagementRate.Value > 100))
                ModelState.AddModelError(nameof(EngagementRate), "Engagement rate phải từ 0 đến 100.");

            if (string.IsNullOrWhiteSpace(Status) ||
                (Status != "Running" && Status != "Paused" && Status != "Completed"))
            {
                Status = "Running";
            }

            if (!ModelState.IsValid)
            {
                await LoadEventsAsync();
                return Page();
            }

            var campaign = new MarketingCampaign
            {
                EventId = EventId,
                AdContent = AdContent,
                AdSpend = AdSpend,
                EngagementRate = EngagementRate ?? 0m,
                Status = Status
            };

            _db.MarketingCampaigns.Add(campaign);
            await _db.SaveChangesAsync();
            _audit.Log(actorId, $"CREATE MarketingCampaign EventId={EventId}", "MarketingCampaigns");
            TempData["Success"] = "Tạo campaign thành công.";
            return RedirectToPage("Index");
        }

        private async AppTask LoadEventsAsync()
        {
            Events = await _db.Events
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            if (Events.Any() && EventId <= 0)
                EventId = Events.First().EventId;
        }
    }
}
