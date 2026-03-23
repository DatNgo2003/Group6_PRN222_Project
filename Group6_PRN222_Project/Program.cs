using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Data;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project_PRN222.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<ProjectPrn222Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ── Session (lưu JWT token phía server) ───────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ── JWT Authentication ─────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Missing JWT configuration. Please set Jwt:Key, Jwt:Issuer, Jwt:Audience in appsettings.json (or environment variables).");
}
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        // Đọc token từ cookie (Razor Pages dùng cookie thay vì header)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.HttpContext.Request.Cookies["auth_token"]
                         ?? ctx.HttpContext.Session.GetString("auth_token");
                if (!string.IsNullOrEmpty(token))
                    ctx.Token = token;
                return System.Threading.Tasks.Task.CompletedTask;
            },
            // Redirect về Login thay vì trả 401 JSON
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.Redirect("/Account/Login?returnUrl=" +
                    Uri.EscapeDataString(ctx.Request.Path));
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

// ── Authorization Policies ─────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("OrganizerOnly", p => p.RequireRole("Organizer"));
    options.AddPolicy("StaffOnly", p => p.RequireRole("Staff(Security)", "Staff(MKT)", "Staff(Logistics)"));
    options.AddPolicy("AnyStaff", p => p.RequireRole(
        "Admin", "Organizer",
        "Staff(Security)", "Staff(MKT)", "Staff(Logistics)"));
});

// ── Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<JwtService>();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();   
builder.Services.AddScoped<IUserService, UserService>();      
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<IRevenueReportService, RevenueReportService>();
builder.Services.AddScoped<IDepartmentKpiService, DepartmentKpiService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDiscountPolicyService, DiscountPolicyService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IFieldReportService, FieldReportService>();
// ================================================================
var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();           // ← Session trước Authentication
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// ── Seed data khi khởi động (chỉ chạy nếu DB trống) ──────────────────
await SeedData.InitializeAsync(app.Services);

app.Run();