using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Group6_PRN222_Project.Models;
using Microsoft.IdentityModel.Tokens;

namespace Group6_PRN222_Project.Auth
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // ── Tạo JWT token từ User ──────────────────────────────────────
        /// <param name="roleClaimValue">Claim role dùng cho phân quyền (sau khi chuẩn hóa từ DB). Mặc định lấy từ user.Role.</param>
        public string GenerateToken(User user, string? roleClaimValue = null)
        {
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "Missing JWT configuration. Please set Jwt:Key, Jwt:Issuer, Jwt:Audience in appsettings.json (or environment variables).");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(
                          int.Parse(_config["Jwt:ExpireHours"] ?? "8"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim("FullName",                user.FullName ?? user.Username),
                new Claim("Email",                   user.Email    ?? ""),
                new Claim(ClaimTypes.Role,           roleClaimValue ?? user.Role?.RoleName ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── Validate và đọc claims từ token ───────────────────────────
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtKey = _config["Jwt:Key"];
                var jwtIssuer = _config["Jwt:Issuer"];
                var jwtAudience = _config["Jwt:Audience"];
                if (string.IsNullOrWhiteSpace(jwtKey) ||
                    string.IsNullOrWhiteSpace(jwtIssuer) ||
                    string.IsNullOrWhiteSpace(jwtAudience))
                {
                    return null;
                }
                var key = Encoding.UTF8.GetBytes(jwtKey);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
