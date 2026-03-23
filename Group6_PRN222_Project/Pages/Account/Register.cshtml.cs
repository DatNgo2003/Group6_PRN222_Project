using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AppTask = System.Threading.Tasks.Task;

namespace Group6_PRN222_Project.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly JwtService _jwt;

        public RegisterModel(ProjectPrn222Context db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập username")]
        [MinLength(3, ErrorMessage = "Username tối thiểu 3 ký tự")]
        public string Username { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // ── Kiểm tra username đã tồn tại chưa ────────────────────
            bool usernameExists = await _db.Users
                .AnyAsync(u => u.Username == Username);
            if (usernameExists)
            {
                ErrorMessage = "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.";
                return Page();
            }

            // ── Kiểm tra email đã tồn tại chưa ───────────────────────
            bool emailExists = await _db.Users
                .AnyAsync(u => u.Email == Email);
            if (emailExists)
            {
                ErrorMessage = "Email này đã được đăng ký. Vui lòng dùng email khác.";
                return Page();
            }

            // ── Lấy RoleId mặc định (Participant hoặc role đầu tiên) ──
            // Tìm role "Participant" nếu có, nếu không thì lấy role có Id thấp nhất
            var defaultRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Participant")
                ?? await _db.Roles.OrderBy(r => r.RoleId).FirstOrDefaultAsync();

            // ── Tạo user mới ──────────────────────────────────────────
            var newUser = new User
            {
                Username = Username.Trim(),
                PasswordHash = Password,        // TODO: Hash bằng BCrypt trước khi lưu
                FullName = FullName.Trim(),
                Email = Email.Trim().ToLower(),
                RoleId = defaultRole?.RoleId,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // ── Load lại user kèm Role để tạo token ───────────────────
            var savedUser = await _db.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.UserId == newUser.UserId);

            // ── Tạo JWT và lưu vào cookie ─────────────────────────────
            var token = _jwt.GenerateToken(savedUser);
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });
            HttpContext.Session.SetString("auth_token", token);
            HttpContext.Session.SetString("username", savedUser.Username);
            HttpContext.Session.SetString("fullname", savedUser.FullName ?? savedUser.Username);
            HttpContext.Session.SetInt32("userid", savedUser.UserId);
            HttpContext.Session.SetString("role", savedUser.Role?.RoleName ?? "");

            // ── Ghi audit log ─────────────────────────────────────────
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = savedUser.UserId,
                Action = "REGISTER",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Chào mừng {savedUser.FullName}! Tài khoản của bạn đã được tạo thành công.";
            return RedirectToPage("/Index");
        }
    }
}
