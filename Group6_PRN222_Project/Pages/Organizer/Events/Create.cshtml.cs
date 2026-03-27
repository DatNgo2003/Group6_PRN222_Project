using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project_PRN222.Helpers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Group6_PRN222_Project.Pages.Organizer.Events
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public CreateModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var statuses = new[] { "Planning", "Ongoing", "Completed", "Cancelled" };
            ViewData["StatusList"] = new SelectList(statuses);
            return Page();
        }

        [BindProperty]
        public Event Event { get; set; } = default!;

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            // Validate StartDate < EndDate
            if (Event.StartDate.HasValue && Event.EndDate.HasValue && Event.StartDate >= Event.EndDate)
            {
                ModelState.AddModelError(string.Empty, "Ngày bắt đầu phải trước ngày kết thúc.");
                var statuses = new[] { "Planning", "Ongoing", "Completed", "Cancelled" };
                ViewData["StatusList"] = new SelectList(statuses);
                return Page();
            }

            // Handle image upload → store as base64 in Images column
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new System.IO.MemoryStream();
                await ImageFile.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                var mimeType = ImageFile.ContentType;
                Event.Images = $"data:{mimeType};base64,{base64}";
            }

            Event.OrganizerId = HttpContext.Session.GetInt32("UserID");
            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
