using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.StaffMKT.Campaigns
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService _audit;

        public IndexModel(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        public List<MarketingCampaign> Campaigns { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        [BindProperty(SupportsGet = true)] public int? FilterEventId { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }

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

            Events = await _db.Events
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            IQueryable<MarketingCampaign> query = _db.MarketingCampaigns
                .AsNoTracking()
                .Include(c => c.Event);

            if (FilterEventId.HasValue)
                query = query.Where(c => c.EventId == FilterEventId.Value);

            if (!string.IsNullOrWhiteSpace(FilterStatus) && !string.Equals(FilterStatus, "All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.Status == FilterStatus);

            Campaigns = await query
                .OrderByDescending(c => c.CampaignId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int campaignId, string status)
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
            status = status?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToPage();
            }

            var campaign = await _db.MarketingCampaigns.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
            if (campaign == null)
            {
                TempData["Error"] = "Không tìm thấy campaign.";
                return RedirectToPage();
            }

            campaign.Status = status;
            await _db.SaveChangesAsync();
            _audit.Log(actorId, $"UPDATE MarketingCampaign ID={campaignId} Status='{status}'", "MarketingCampaigns");
            TempData["Success"] = "Cập nhật trạng thái campaign thành công.";
            return RedirectToPage();
        }
    }
}
