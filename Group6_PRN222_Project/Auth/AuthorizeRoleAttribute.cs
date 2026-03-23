using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;

namespace Group6_PRN222_Project.Auth
{
    /// <summary>
    /// Attribute kiểm tra JWT từ Cookie/Session và phân quyền theo Role.
    /// Dùng: [AuthorizeRole("Admin")] hoặc [AuthorizeRole("Admin","Organizer")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeRoleAttribute : Attribute, IPageFilter, IFilterMetadata
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext ctx) { }
        public void OnPageHandlerExecuted(PageHandlerExecutedContext ctx) { }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext ctx)
        {
            var httpCtx = ctx.HttpContext;

            // ── Lấy token từ cookie hoặc session ──────────────────────
            var token = httpCtx.Request.Cookies["auth_token"]
                     ?? httpCtx.Session.GetString("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                Redirect(ctx, "/Account/Login", httpCtx.Request.Path);
                return;
            }

            // ── Decode token (không validate lại — Program.cs đã lo) ──
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // Kiểm tra hết hạn
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    httpCtx.Response.Cookies.Delete("auth_token");
                    httpCtx.Session.Clear();
                    Redirect(ctx, "/Account/Login", httpCtx.Request.Path);
                    return;
                }

                // Lấy role từ claim
                var roleClaim = jwt.Claims
                    .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                                      || c.Type == "role")?.Value ?? "";

                // Kiểm tra role có trong danh sách được phép không
                if (_roles.Length > 0 && !_roles.Contains(roleClaim))
                {
                    ctx.Result = new RedirectToPageResult("/AccessDenied");
                    return;
                }
            }
            catch
            {
                Redirect(ctx, "/Account/Login", httpCtx.Request.Path);
            }
        }

        private static void Redirect(PageHandlerExecutingContext ctx, string page, string returnUrl)
        {
            ctx.Result = new RedirectToPageResult(page,
                new { returnUrl = returnUrl });
        }
    }
}
