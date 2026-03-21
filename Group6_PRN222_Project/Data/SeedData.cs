using Group6_PRN222_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Data
{
    public static class SeedData
    {
        public static async System.Threading.Tasks.Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProjectPrn222Context>();

            await db.Database.EnsureCreatedAsync();

            // Nếu đã có data thì bỏ qua
            if (await db.Roles.AnyAsync()) return;

            // =============================================
            // 1. ROLES
            // =============================================
            var roles = new List<Role>
            {
                new Role { RoleName = "Admin",            Status = "Active" },
                new Role { RoleName = "Organizer",        Status = "Active" },
                new Role { RoleName = "Staff(Security)",  Status = "Active" },
                new Role { RoleName = "Staff(MKT)",       Status = "Active" },
                new Role { RoleName = "Staff(Logistics)", Status = "Active" },
                new Role { RoleName = "Participant",      Status = "Active" },
            };
            db.Roles.AddRange(roles);
            await db.SaveChangesAsync();

            // =============================================
            // 2. DEPARTMENTS
            // =============================================
            var departments = new List<Department>
            {
                new Department { DepartmentName = "Ban Quản trị",  Status = "Active" },
                new Department { DepartmentName = "Ban Tổ chức",   Status = "Active" },
                new Department { DepartmentName = "Ban An ninh",   Status = "Active" },
                new Department { DepartmentName = "Ban Marketing",  Status = "Active" },
                new Department { DepartmentName = "Ban Hậu cần",   Status = "Active" },
            };
            db.Departments.AddRange(departments);
            await db.SaveChangesAsync();

            var roleAdmin = roles.First(r => r.RoleName == "Admin");
            var roleOrg = roles.First(r => r.RoleName == "Organizer");
            var roleSecurity = roles.First(r => r.RoleName == "Staff(Security)");
            var roleMkt = roles.First(r => r.RoleName == "Staff(MKT)");
            var roleLogistics = roles.First(r => r.RoleName == "Staff(Logistics)");
            var roleParticipant = roles.First(r => r.RoleName == "Participant");

            // =============================================
            // 3. USERS
            // =============================================
            var users = new List<User>
            {
                new User
                {
                    Username     = "admin",
                    PasswordHash = "Admin@123",
                    FullName     = "Nguyễn Quản Trị",
                    Email        = "admin@eventpro.vn",
                    RoleId       = roleAdmin.RoleId,
                    DepartmentId = departments[0].DepartmentId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
                new User
                {
                    Username     = "organizer1",
                    PasswordHash = "Org@123",
                    FullName     = "Trần Thị Tổ Chức",
                    Email        = "organizer@eventpro.vn",
                    RoleId       = roleOrg.RoleId,
                    DepartmentId = departments[1].DepartmentId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
                new User
                {
                    Username     = "security1",
                    PasswordHash = "Staff@123",
                    FullName     = "Lê Văn An Ninh",
                    Email        = "security@eventpro.vn",
                    RoleId       = roleSecurity.RoleId,
                    DepartmentId = departments[2].DepartmentId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
                new User
                {
                    Username     = "mkt1",
                    PasswordHash = "Staff@123",
                    FullName     = "Phạm Thị Marketing",
                    Email        = "mkt@eventpro.vn",
                    RoleId       = roleMkt.RoleId,
                    DepartmentId = departments[3].DepartmentId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
                new User
                {
                    Username     = "logistics1",
                    PasswordHash = "Staff@123",
                    FullName     = "Hoàng Văn Hậu Cần",
                    Email        = "logistics@eventpro.vn",
                    RoleId       = roleLogistics.RoleId,
                    DepartmentId = departments[4].DepartmentId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
                new User
                {
                    Username     = "user1",
                    PasswordHash = "User@123",
                    FullName     = "Nguyễn Văn Tham Dự",
                    Email        = "user1@gmail.com",
                    RoleId       = roleParticipant.RoleId,
                    CreatedAt    = DateTime.Now,
                    Status       = "Active",
                },
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();

            var userAdmin = users.First(u => u.Username == "admin");
            var userOrg = users.First(u => u.Username == "organizer1");
            var userSecurity = users.First(u => u.Username == "security1");
            var userMkt = users.First(u => u.Username == "mkt1");
            var userLogistics = users.First(u => u.Username == "logistics1");

            // =============================================
            // 4. EVENTS
            // =============================================
            var events = new List<Event>
            {
                new Event
                {
                    EventName   = "Tech Summit 2025",
                    Concept     = "Hội nghị công nghệ lớn nhất miền Bắc năm 2025",
                    Location    = "Trung tâm Hội nghị Quốc gia, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(15),
                    EndDate     = DateTime.Now.AddDays(16),
                    Status      = "Active",
                    OrganizerId = userOrg.UserId,
                },
                new Event
                {
                    EventName   = "Music Festival Mùa Hè",
                    Concept     = "Lễ hội âm nhạc ngoài trời với 20+ nghệ sĩ",
                    Location    = "Công viên Thống Nhất, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(30),
                    EndDate     = DateTime.Now.AddDays(30),
                    Status      = "Active",
                    OrganizerId = userOrg.UserId,
                },
                new Event
                {
                    EventName   = "Startup Pitch Night",
                    Concept     = "Đêm gọi vốn cho các startup công nghệ",
                    Location    = "FPT Tower, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(7),
                    EndDate     = DateTime.Now.AddDays(7),
                    Status      = "Planning",
                    OrganizerId = userOrg.UserId,
                },
                new Event
                {
                    EventName   = "Workshop AI & Machine Learning",
                    Concept     = "Workshop thực hành AI cho lập trình viên",
                    Location    = "FPT University, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(3),
                    EndDate     = DateTime.Now.AddDays(3),
                    Status      = "Active",
                    OrganizerId = userOrg.UserId,
                },
                new Event
                {
                    EventName   = "Triển lãm Nghệ thuật Đương đại",
                    Concept     = "Triển lãm tác phẩm của 50 nghệ sĩ trẻ Việt Nam",
                    Location    = "Bảo tàng Mỹ thuật, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(20),
                    EndDate     = DateTime.Now.AddDays(25),
                    Status      = "Planning",
                    OrganizerId = userOrg.UserId,
                },
                new Event
                {
                    EventName   = "Hội thảo Chuyển đổi Số",
                    Concept     = "Chia sẻ kinh nghiệm chuyển đổi số cho doanh nghiệp",
                    Location    = "Khách sạn Melia, Hà Nội",
                    StartDate   = DateTime.Now.AddDays(45),
                    EndDate     = DateTime.Now.AddDays(45),
                    Status      = "Active",
                    OrganizerId = userOrg.UserId,
                },
            };
            db.Events.AddRange(events);
            await db.SaveChangesAsync();

            // =============================================
            // 5. BUDGETS
            // =============================================
            var rnd = new Random();
            var budgets = events.Select(e => new Budget
            {
                EventId = e.EventId,
                TotalAllocated = 50_000_000 + (rnd.Next(1, 10) * 10_000_000),
                SpentAmount = 10_000_000,
                ApprovalStatus = "Approved",
                ApprovedBy = userAdmin.UserId,
            }).ToList();
            db.Budgets.AddRange(budgets);
            await db.SaveChangesAsync();

            // =============================================
            // 6. PARTICIPANTS
            // =============================================
            var participants = new List<Participant>
            {
                new Participant { FullName = "Nguyễn Minh Anh",  Email = "minhanh@gmail.com",  Phone = "0901234567", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Trần Hữu Đức",     Email = "huuduc@gmail.com",   Phone = "0912345678", IsVip = true,  IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Lê Thị Lan",       Email = "thilan@gmail.com",   Phone = "0923456789", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Phạm Văn Tuấn",    Email = "vantuan@gmail.com",  Phone = "0934567890", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Hoàng Thu Hà",     Email = "thuha@gmail.com",    Phone = "0945678901", IsVip = true,  IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Đỗ Quốc Bảo",     Email = "quocbao@gmail.com",  Phone = "0956789012", IsVip = false, IsBlacklisted = true,  Status = "Active" },
            };
            db.Participants.AddRange(participants);
            await db.SaveChangesAsync();

            // =============================================
            // 7. TICKETS
            // =============================================
            var ev1 = events[0];
            var ev2 = events[1];
            var tickets = new List<Ticket>
            {
                new Ticket { EventId = ev1.EventId, ParticipantId = participants[0].ParticipantId, Qrcode = Guid.NewGuid(), PaymentStatus = "Paid",    CheckInTime = null,          Status = "Active" },
                new Ticket { EventId = ev1.EventId, ParticipantId = participants[1].ParticipantId, Qrcode = Guid.NewGuid(), PaymentStatus = "Paid",    CheckInTime = DateTime.Now,  Status = "Active" },
                new Ticket { EventId = ev1.EventId, ParticipantId = participants[2].ParticipantId, Qrcode = Guid.NewGuid(), PaymentStatus = "Pending", CheckInTime = null,          Status = "Active" },
                new Ticket { EventId = ev2.EventId, ParticipantId = participants[3].ParticipantId, Qrcode = Guid.NewGuid(), PaymentStatus = "Paid",    CheckInTime = null,          Status = "Active" },
                new Ticket { EventId = ev2.EventId, ParticipantId = participants[4].ParticipantId, Qrcode = Guid.NewGuid(), PaymentStatus = "Paid",    CheckInTime = null,          Status = "Active" },
            };
            db.Tickets.AddRange(tickets);
            await db.SaveChangesAsync();

            // =============================================
            // 8. TASKS
            // =============================================
            var tasks = new List<Group6_PRN222_Project.Models.EventTask>
            {
                new Group6_PRN222_Project.Models.EventTask
                {
                    EventId     = ev1.EventId,
                    TaskName    = "Chuẩn bị danh sách VIP",
                    Description = "Tổng hợp và xác nhận danh sách khách VIP cho sự kiện",
                    Deadline    = DateTime.Now.AddDays(10),
                    Status      = "In Progress",
                    AssignedTo  = userSecurity.UserId,
                },
                new Group6_PRN222_Project.Models.EventTask
                {
                    EventId     = ev1.EventId,
                    TaskName    = "Chạy quảng cáo Facebook",
                    Description = "Thiết lập và chạy chiến dịch quảng cáo trên Facebook Ads",
                    Deadline    = DateTime.Now.AddDays(5),
                    Status      = "To Do",
                    AssignedTo  = userMkt.UserId,
                },
                new Group6_PRN222_Project.Models.EventTask
                {
                    EventId     = ev1.EventId,
                    TaskName    = "Kiểm tra thiết bị âm thanh",
                    Description = "Kiểm tra toàn bộ hệ thống âm thanh, ánh sáng sân khấu",
                    Deadline    = DateTime.Now.AddDays(14),
                    Status      = "To Do",
                    AssignedTo  = userLogistics.UserId,
                },
            };
            db.Tasks.AddRange(tasks);
            await db.SaveChangesAsync();

            // =============================================
            // 9. MARKETING CAMPAIGNS
            // =============================================
            var campaigns = new List<MarketingCampaign>
            {
                new MarketingCampaign
                {
                    EventId        = ev1.EventId,
                    AdContent      = "Tech Summit 2025 - Sự kiện công nghệ không thể bỏ lỡ!",
                    AdSpend        = 5_000_000,
                    EngagementRate = 4.5m,
                    Status         = "Active",
                },
                new MarketingCampaign
                {
                    EventId        = ev2.EventId,
                    AdContent      = "Music Festival Mùa Hè - 20+ nghệ sĩ, 1 đêm không quên!",
                    AdSpend        = 8_000_000,
                    EngagementRate = 6.2m,
                    Status         = "Active",
                },
            };
            db.MarketingCampaigns.AddRange(campaigns);
            await db.SaveChangesAsync();

            // =============================================
            // 10. AUDIT LOG - ghi nhận seed
            // =============================================
            db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = userAdmin.UserId,
                Action = "SEED_DATA",
                TableName = "ALL",
                ActionTime = DateTime.Now,
                Status = "Success",
            });
            await db.SaveChangesAsync();

            Console.WriteLine("✅ Seed data hoàn thành!");
        }
    }
}