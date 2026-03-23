using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public LoginModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (SessionHelper.IsOrganizer(HttpContext.Session))
                return RedirectToPage("/Organizer/Dashboard");
                
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password.";
                return Page();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == Username);

            if (user == null)
            {
                ErrorMessage = "Invalid username.";
                return Page();
            }

            bool isPasswordCorrect = PasswordHasher.Verify(Password, user.PasswordHash ?? "") 
                                  || user.PasswordHash == "hash_" + Password
                                  || user.PasswordHash == Password;

            if (!isPasswordCorrect)
            {
                ErrorMessage = "Invalid password.";
                return Page();
            }

            string roleName = user.Role?.RoleName ?? "";
            
            // Set fallback role names for seeded data
            if (string.IsNullOrEmpty(roleName))
            {
                if (user.RoleId == 2 || user.Username.Contains("organizer")) roleName = "Organizer";
                else if (user.RoleId == null || user.Username.Contains("admin")) roleName = "Admin";
            }

            if (roleName != "Event Manager" && roleName.ToLower() != "admin")
            {
                ErrorMessage = "Access Denied. Only Organizers can log into this portal.";
                return Page();
            }

            SessionHelper.SetUser(HttpContext.Session, user.UserId, user.Username, user.FullName ?? "", roleName);
            
            return RedirectToPage("/Organizer/Dashboard");
        }
    }
}
