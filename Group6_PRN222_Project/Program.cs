// ================================================================
// Program.cs — thêm các dòng này vào file Program.cs hiện có
// ================================================================
using Group6_PRN222_Project.Models;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext (đã có từ scaffold — giữ nguyên connection string)
builder.Services.AddDbContext<ProjectPrn222Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// 2. Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// 3. Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EMS.Session";
});

// 4. DI — đăng ký Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
// Sau thêm tiếp:
// builder.Services.AddScoped<IEventService, EventService>();
// builder.Services.AddScoped<IAuthService,  AuthService>();

// ================================================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// QUAN TRỌNG: UseSession phải trước MapRazorPages
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();

app.Run();
