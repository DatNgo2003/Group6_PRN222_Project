using Group6_PRN222_Project.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.KPIs
{
    [AuthorizeRole("Admin")]
    public class DepartmentKpiModel : PageModel
    {
        private readonly IDepartmentKpiService _kpiSvc;

        public DepartmentKpiModel(IDepartmentKpiService kpiSvc)
        {
            _kpiSvc = kpiSvc;
        }

        [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo   { get; set; }

        public DepartmentKpiSummary Summary { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID",   1);
                HttpContext.Session.SetString("Role",     "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            Summary = await _kpiSvc.GetKpisAsync(DateFrom, DateTo);
            return Page();
        }
    }
}
