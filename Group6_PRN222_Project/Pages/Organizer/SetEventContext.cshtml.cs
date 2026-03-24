using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.Organizer
{
    public class SetEventContextModel : PageModel
    {
        public IActionResult OnPost(int? eventId)
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            if (eventId.HasValue)
            {
                HttpContext.Session.SetInt32("SelectedEventId", eventId.Value);
            }
            else
            {
                HttpContext.Session.Remove("SelectedEventId");
            }
            
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToPage("/Organizer/Dashboard");
            }
            return Redirect(referer);
        }
    }
}
