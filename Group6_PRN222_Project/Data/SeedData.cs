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
                new User { Username = "admin",      PasswordHash = "Admin@123", FullName = "Nguyễn Quản Trị",    Email = "admin@eventpro.vn",     RoleId = roleAdmin.RoleId,       DepartmentId = departments[0].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "organizer1", PasswordHash = "Org@123",   FullName = "Trần Thị Tổ Chức",   Email = "organizer@eventpro.vn", RoleId = roleOrg.RoleId,         DepartmentId = departments[1].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "organizer2", PasswordHash = "Org@123",   FullName = "Nguyễn Văn Sự Kiện", Email = "org2@eventpro.vn",      RoleId = roleOrg.RoleId,         DepartmentId = departments[1].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "security1",  PasswordHash = "Staff@123", FullName = "Lê Văn An Ninh",     Email = "security@eventpro.vn",  RoleId = roleSecurity.RoleId,    DepartmentId = departments[2].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "mkt1",       PasswordHash = "Staff@123", FullName = "Phạm Thị Marketing", Email = "mkt@eventpro.vn",       RoleId = roleMkt.RoleId,         DepartmentId = departments[3].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "logistics1", PasswordHash = "Staff@123", FullName = "Hoàng Văn Hậu Cần", Email = "logistics@eventpro.vn", RoleId = roleLogistics.RoleId,   DepartmentId = departments[4].DepartmentId, CreatedAt = DateTime.Now, Status = "Active" },
                new User { Username = "user1",      PasswordHash = "User@123",  FullName = "Nguyễn Văn Tham Dự", Email = "user1@gmail.com",       RoleId = roleParticipant.RoleId, DepartmentId = null,                        CreatedAt = DateTime.Now, Status = "Active" },
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();

            var userAdmin = users.First(u => u.Username == "admin");
            var userOrg = users.First(u => u.Username == "organizer1");
            var userOrg2 = users.First(u => u.Username == "organizer2");
            var userSecurity = users.First(u => u.Username == "security1");
            var userMkt = users.First(u => u.Username == "mkt1");
            var userLogistics = users.First(u => u.Username == "logistics1");

            // =============================================
            // 4. EVENTS — StartDate trải đều để Revenue chart có data
            // =============================================
            var events = new List<Event>
            {
                new Event { EventName = "Tech Summit 2025",               Concept = "Hội nghị công nghệ lớn nhất miền Bắc năm 2025",    Location = "Trung tâm Hội nghị Quốc gia, Hà Nội", StartDate = DateTime.Now.AddMonths(-3),           EndDate = DateTime.Now.AddMonths(-3).AddDays(1),  Status = "Completed", OrganizerId = userOrg.UserId  },
                new Event { EventName = "Music Festival Mùa Hè",          Concept = "Lễ hội âm nhạc ngoài trời với 20+ nghệ sĩ",         Location = "Công viên Thống Nhất, Hà Nội",        StartDate = DateTime.Now.AddMonths(-2),           EndDate = DateTime.Now.AddMonths(-2),             Status = "Completed", OrganizerId = userOrg.UserId  },
                new Event { EventName = "Startup Pitch Night",            Concept = "Đêm gọi vốn cho các startup công nghệ",             Location = "FPT Tower, Hà Nội",                   StartDate = DateTime.Now.AddMonths(-2).AddDays(15),EndDate = DateTime.Now.AddMonths(-2).AddDays(15), Status = "Completed", OrganizerId = userOrg2.UserId },
                new Event { EventName = "Workshop AI & Machine Learning", Concept = "Workshop thực hành AI cho lập trình viên",           Location = "FPT University, Hà Nội",              StartDate = DateTime.Now.AddMonths(-1),           EndDate = DateTime.Now.AddMonths(-1),             Status = "Completed", OrganizerId = userOrg.UserId  },
                new Event { EventName = "Triển lãm Nghệ thuật Đương đại", Concept = "Triển lãm tác phẩm của 50 nghệ sĩ trẻ Việt Nam",   Location = "Bảo tàng Mỹ thuật, Hà Nội",          StartDate = DateTime.Now.AddMonths(-1).AddDays(10),EndDate = DateTime.Now.AddMonths(-1).AddDays(15), Status = "Completed", OrganizerId = userOrg2.UserId },
                new Event { EventName = "Hội thảo Chuyển đổi Số",         Concept = "Chia sẻ kinh nghiệm chuyển đổi số cho doanh nghiệp",Location = "Khách sạn Melia, Hà Nội",             StartDate = DateTime.Now.AddDays(-3),             EndDate = DateTime.Now.AddDays(-3),               Status = "Active",    OrganizerId = userOrg.UserId  },
                // Sắp tới — có Budget Pending để test duyệt
                new Event { EventName = "Summer Music Gala 2025",         Concept = "Đại nhạc hội mùa hè, 30+ nghệ sĩ",                 Location = "Sân vận động Mỹ Đình, Hà Nội",       StartDate = DateTime.Now.AddDays(20),             EndDate = DateTime.Now.AddDays(21),               Status = "Planning",  OrganizerId = userOrg.UserId  },
                new Event { EventName = "Vietnam Dev Conf 2025",          Concept = "Hội nghị lập trình viên Việt Nam",                  Location = "GEM Center, TP.HCM",                  StartDate = DateTime.Now.AddDays(35),             EndDate = DateTime.Now.AddDays(36),               Status = "Planning",  OrganizerId = userOrg2.UserId },
            };
            db.Events.AddRange(events);
            await db.SaveChangesAsync();

            // =============================================
            // 5. BUDGETS — đủ 3 trạng thái để test filter
            // =============================================
            var budgets = new List<Budget>
            {
                // Approved
                new Budget { EventId = events[0].EventId, TotalAllocated = 120_000_000, SpentAmount = 95_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                new Budget { EventId = events[1].EventId, TotalAllocated = 80_000_000,  SpentAmount = 72_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                new Budget { EventId = events[2].EventId, TotalAllocated = 50_000_000,  SpentAmount = 31_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                new Budget { EventId = events[3].EventId, TotalAllocated = 35_000_000,  SpentAmount = 28_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                new Budget { EventId = events[4].EventId, TotalAllocated = 60_000_000,  SpentAmount = 45_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                new Budget { EventId = events[5].EventId, TotalAllocated = 70_000_000,  SpentAmount = 18_000_000, ApprovalStatus = "Approved", ApprovedBy = userAdmin.UserId },
                // Rejected
                new Budget { EventId = events[5].EventId, TotalAllocated = 200_000_000, SpentAmount = 0,          ApprovalStatus = "Rejected", ApprovedBy = userAdmin.UserId },
                // Pending — Admin cần vào duyệt
                new Budget { EventId = events[6].EventId, TotalAllocated = 150_000_000, SpentAmount = 0,          ApprovalStatus = "Pending",  ApprovedBy = null },
                new Budget { EventId = events[7].EventId, TotalAllocated = 95_000_000,  SpentAmount = 0,          ApprovalStatus = "Pending",  ApprovedBy = null },
                // events[6] có 2 budget để test panel "Ngân sách cùng sự kiện"
                new Budget { EventId = events[6].EventId, TotalAllocated = 25_000_000,  SpentAmount = 0,          ApprovalStatus = "Pending",  ApprovedBy = null },
            };
            db.Budgets.AddRange(budgets);
            await db.SaveChangesAsync();

            // =============================================
            // 6. PARTICIPANTS
            // =============================================
            var participants = new List<Participant>
            {
                new Participant { FullName = "Nguyễn Minh Anh",  Email = "minhanh@gmail.com",   Phone = "0901234567", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Trần Hữu Đức",     Email = "huuduc@gmail.com",    Phone = "0912345678", IsVip = true,  IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Lê Thị Lan",       Email = "thilan@gmail.com",    Phone = "0923456789", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Phạm Văn Tuấn",    Email = "vantuan@gmail.com",   Phone = "0934567890", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Hoàng Thu Hà",     Email = "thuha@gmail.com",     Phone = "0945678901", IsVip = true,  IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Đỗ Quốc Bảo",     Email = "quocbao@gmail.com",   Phone = "0956789012", IsVip = false, IsBlacklisted = true,  Status = "Active" },
                new Participant { FullName = "Vũ Thị Hương",     Email = "vuhuong@gmail.com",   Phone = "0967890123", IsVip = false, IsBlacklisted = false, Status = "Active" },
                new Participant { FullName = "Bùi Thanh Long",   Email = "thanhlong@gmail.com", Phone = "0978901234", IsVip = true,  IsBlacklisted = false, Status = "Active" },
            };
            db.Participants.AddRange(participants);
            await db.SaveChangesAsync();

            // =============================================
            // 7. TICKETS — rải đều theo event để Revenue chart có data
            // =============================================
            var payPattern = new[] { "Paid", "Paid", "Paid", "Paid", "Pending" };
            var tickets = new List<Ticket>();

            void AddTickets(Event ev, int count)
            {
                for (int i = 0; i < count; i++)
                    tickets.Add(new Ticket
                    {
                        EventId = ev.EventId,
                        ParticipantId = participants[i % participants.Count].ParticipantId,
                        Qrcode = Guid.NewGuid(),
                        PaymentStatus = payPattern[i % payPattern.Length],
                        CheckInTime = null,
                        Status = "Valid"
                    });
            }

            AddTickets(events[0], 8);  // Tech Summit — 3 tháng trước
            AddTickets(events[1], 10); // Music Festival — 2 tháng trước
            AddTickets(events[2], 6);  // Startup Pitch — 2 tháng trước
            AddTickets(events[3], 7);  // Workshop AI — tháng trước
            AddTickets(events[4], 5);  // Triển lãm — tháng trước
            AddTickets(events[5], 8);  // Hội thảo — tháng này

            db.Tickets.AddRange(tickets);
            await db.SaveChangesAsync();

            // =============================================
            // 8. TASKS
            // =============================================
            var tasks = new List<Group6_PRN222_Project.Models.Task>
            {
                new Group6_PRN222_Project.Models.Task { EventId = events[0].EventId, TaskName = "Chuẩn bị danh sách VIP",     Description = "Tổng hợp và xác nhận danh sách khách VIP cho sự kiện",          Deadline = DateTime.Now.AddDays(10), Status = "In Progress", AssignedTo = userSecurity.UserId },
                new Group6_PRN222_Project.Models.Task { EventId = events[0].EventId, TaskName = "Chạy quảng cáo Facebook",    Description = "Thiết lập và chạy chiến dịch quảng cáo trên Facebook Ads",       Deadline = DateTime.Now.AddDays(5),  Status = "To Do",       AssignedTo = userMkt.UserId      },
                new Group6_PRN222_Project.Models.Task { EventId = events[0].EventId, TaskName = "Kiểm tra thiết bị âm thanh", Description = "Kiểm tra toàn bộ hệ thống âm thanh, ánh sáng sân khấu",         Deadline = DateTime.Now.AddDays(14), Status = "To Do",       AssignedTo = userLogistics.UserId},
                new Group6_PRN222_Project.Models.Task { EventId = events[6].EventId, TaskName = "Setup sân khấu chính",       Description = "Lắp đặt và kiểm tra toàn bộ sân khấu chính cho sự kiện",        Deadline = DateTime.Now.AddDays(18), Status = "To Do",       AssignedTo = userLogistics.UserId},
                new Group6_PRN222_Project.Models.Task { EventId = events[7].EventId, TaskName = "Thiết kế backdrop sự kiện",  Description = "Thiết kế và in ấn backdrop theo brief từ ban tổ chức",           Deadline = DateTime.Now.AddDays(28), Status = "To Do",       AssignedTo = userMkt.UserId      },
            };
            db.Tasks.AddRange(tasks);
            await db.SaveChangesAsync();

            // =============================================
            // 9. MARKETING CAMPAIGNS
            // =============================================
            var campaigns = new List<MarketingCampaign>
            {
                new MarketingCampaign { EventId = events[0].EventId, AdContent = "Tech Summit 2025 - Sự kiện công nghệ không thể bỏ lỡ!", AdSpend = 5_000_000,  EngagementRate = 4.5m, Status = "Completed" },
                new MarketingCampaign { EventId = events[1].EventId, AdContent = "Music Festival Mùa Hè - 20+ nghệ sĩ, 1 đêm không quên!", AdSpend = 8_000_000, EngagementRate = 6.2m, Status = "Completed" },
                new MarketingCampaign { EventId = events[6].EventId, AdContent = "Summer Music Gala 2025 — Coming soon!",                   AdSpend = 12_000_000,EngagementRate = 7.1m, Status = "Running"   },
                new MarketingCampaign { EventId = events[7].EventId, AdContent = "Vietnam Dev Conf 2025 — Call for speakers!",              AdSpend = 6_000_000, EngagementRate = 5.4m, Status = "Running"   },
            };
            db.MarketingCampaigns.AddRange(campaigns);
            await db.SaveChangesAsync();

            // =============================================
            // 10. EQUIPMENTS
            // =============================================
            var equipments = new List<Equipment>
            {
                new Equipment { EquipmentName = "Màn hình LED P3 (3x2m)",    TotalOwned = 4,  AvailableQuantity = 4 },
                new Equipment { EquipmentName = "Hệ thống âm thanh JBL",      TotalOwned = 2,  AvailableQuantity = 2 },
                new Equipment { EquipmentName = "Máy chiếu 10.000 lumen",     TotalOwned = 3,  AvailableQuantity = 3 },
                new Equipment { EquipmentName = "Mic không dây Shure",         TotalOwned = 20, AvailableQuantity = 20 },
                new Equipment { EquipmentName = "Đèn sân khấu Moving Head",   TotalOwned = 16, AvailableQuantity = 16 },
                new Equipment { EquipmentName = "Bàn mixer âm thanh 32ch",    TotalOwned = 2,  AvailableQuantity = 0, DefectiveQuantity = 2 },
                new Equipment { EquipmentName = "Camera livestream 4K",        TotalOwned = 5,  AvailableQuantity = 5 },
                new Equipment { EquipmentName = "Máy phát điện dự phòng",     TotalOwned = 1,  AvailableQuantity = 0, DefectiveQuantity = 1 },
                new Equipment { EquipmentName = "Cổng check-in QR",           TotalOwned = 8,  AvailableQuantity = 8 },
                new Equipment { EquipmentName = "Bộ đàm Motorola (set 10)",   TotalOwned = 3,  AvailableQuantity = 0, DefectiveQuantity = 3 },
            };
            db.Equipments.AddRange(equipments);
            await db.SaveChangesAsync();

            // =============================================
            // 11. SURVEY RESPONSES
            // =============================================
            var surveys = new List<SurveyResponse>
            {
                new SurveyResponse { EventId = events[0].EventId, ParticipantId = participants[0].ParticipantId, Rating = 5, Comments = "Nội dung rất hay, diễn giả chuyên nghiệp!", Status = "Completed" },
                new SurveyResponse { EventId = events[0].EventId, ParticipantId = participants[1].ParticipantId, Rating = 4, Comments = "Tổ chức tốt, địa điểm thuận tiện.",           Status = "Completed" },
                new SurveyResponse { EventId = events[0].EventId, ParticipantId = participants[2].ParticipantId, Rating = 5, Comments = "Xuất sắc! Sẽ tham dự lần sau.",               Status = "Completed" },
                new SurveyResponse { EventId = events[0].EventId, ParticipantId = participants[3].ParticipantId, Rating = 3, Comments = "Âm thanh hơi nhỏ ở khu vực cuối phòng.",     Status = "Completed" },
                new SurveyResponse { EventId = events[1].EventId, ParticipantId = participants[4].ParticipantId, Rating = 5, Comments = "Âm nhạc cực đỉnh, nghệ sĩ biểu diễn tuyệt!", Status = "Completed" },
                new SurveyResponse { EventId = events[1].EventId, ParticipantId = participants[5].ParticipantId, Rating = 4, Comments = "Hơi đông nhưng không khí rất vui.",           Status = "Completed" },
                new SurveyResponse { EventId = events[1].EventId, ParticipantId = participants[0].ParticipantId, Rating = 5, Comments = "Một đêm không thể quên!",                    Status = "Completed" },
                new SurveyResponse { EventId = events[2].EventId, ParticipantId = participants[1].ParticipantId, Rating = 4, Comments = "Pitch deck chất lượng, nhiều startup tiềm năng.", Status = "Completed" },
                new SurveyResponse { EventId = events[2].EventId, ParticipantId = participants[2].ParticipantId, Rating = 3, Comments = "Thời gian Q&A quá ngắn.",                   Status = "Completed" },
                new SurveyResponse { EventId = events[3].EventId, ParticipantId = participants[3].ParticipantId, Rating = 5, Comments = "Workshop thực hành rất bổ ích!",             Status = "Completed" },
                new SurveyResponse { EventId = events[3].EventId, ParticipantId = participants[4].ParticipantId, Rating = 5, Comments = "Instructor rất nhiệt tình, code examples rõ ràng.", Status = "Completed" },
                new SurveyResponse { EventId = events[4].EventId, ParticipantId = participants[5].ParticipantId, Rating = 4, Comments = "Triển lãm đẹp, tác phẩm đa dạng.",           Status = "Completed" },
                new SurveyResponse { EventId = events[5].EventId, ParticipantId = participants[0].ParticipantId, Rating = 4, Comments = "Nội dung thiết thực cho doanh nghiệp vừa và nhỏ.", Status = "Completed" },
                new SurveyResponse { EventId = events[5].EventId, ParticipantId = participants[1].ParticipantId, Rating = 2, Comments = "Phòng hội thảo hơi nóng, điều hòa yếu.",    Status = "Completed" },
            };
            db.SurveyResponses.AddRange(surveys);
            await db.SaveChangesAsync();

            // =============================================
            // 12. FIELD REPORTS
            // =============================================
            var fieldReports = new List<FieldReport>
            {
                new FieldReport { EventId = events[0].EventId, StaffId = userSecurity.UserId,  ReportType = "Security",  Content = "Kiểm tra an ninh vòng ngoài OK. Phát hiện 1 vé giả tại cổng B, đã xử lý.", ReportTime = events[0].StartDate?.AddHours(1),  Status = "Submitted" },
                new FieldReport { EventId = events[0].EventId, StaffId = userLogistics.UserId, ReportType = "Logistics", Content = "Hệ thống âm thanh hoạt động ổn định. Màn hình LED P3 góc trái bị lag nhẹ, đã restart.", ReportTime = events[0].StartDate?.AddHours(2), Status = "Submitted" },
                new FieldReport { EventId = events[0].EventId, StaffId = userMkt.UserId,       ReportType = "Marketing", Content = "Livestream đạt 2.400 người xem đồng thời. Hashtag #TechSummit2025 trending top 3.", ReportTime = events[0].StartDate?.AddHours(3), Status = "Submitted" },
                new FieldReport { EventId = events[1].EventId, StaffId = userSecurity.UserId,  ReportType = "Security",  Content = "Không có sự cố an ninh. Khách vào ra trật tự.", ReportTime = events[1].StartDate?.AddHours(1), Status = "Submitted" },
                new FieldReport { EventId = events[1].EventId, StaffId = userLogistics.UserId, ReportType = "Logistics", Content = "Setup sân khấu hoàn thành trước giờ G 2 tiếng. Tất cả thiết bị hoạt động tốt.", ReportTime = events[1].StartDate?.AddMinutes(-120), Status = "Submitted" },
                new FieldReport { EventId = events[3].EventId, StaffId = userLogistics.UserId, ReportType = "Logistics", Content = "Máy tính lab đầy đủ 30/30 máy. Mạng internet ổn định suốt workshop.", ReportTime = events[3].StartDate?.AddHours(1), Status = "Submitted" },
                new FieldReport { EventId = events[5].EventId, StaffId = userSecurity.UserId,  ReportType = "Security",  Content = "Check-in QR hoạt động tốt. 320/350 đại biểu đã vào hội trường.", ReportTime = events[5].StartDate?.AddMinutes(30), Status = "Submitted" },
                new FieldReport { EventId = events[5].EventId, StaffId = userMkt.UserId,       ReportType = "Marketing", Content = "Press release đã gửi cho 15 báo. Có 3 phóng viên trực tiếp tại hội trường.", ReportTime = events[5].StartDate?.AddHours(2), Status = "Submitted" },
            };
            db.FieldReports.AddRange(fieldReports);
            await db.SaveChangesAsync();

            // =============================================
            // 13. AUDIT LOGS
            // =============================================
            db.SystemAuditLogs.AddRange(new List<SystemAuditLog>
            {
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "SEED_DATA",                          TableName = "ALL",         ActionTime = DateTime.Now,                Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "LOGIN_STAFF_ADMIN",                  TableName = "Users",       ActionTime = DateTime.Now.AddMinutes(-30), Status = "Recorded" },
                new SystemAuditLog { UserId = userOrg.UserId,   Action = "LOGIN_STAFF_ORGANIZER",              TableName = "Users",       ActionTime = DateTime.Now.AddHours(-2),   Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "APPROVE Budget ID=1",                TableName = "Budgets",     ActionTime = DateTime.Now.AddDays(-10),   Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "APPROVE Budget ID=2",                TableName = "Budgets",     ActionTime = DateTime.Now.AddDays(-8),    Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "REJECT Budget ID=7",                 TableName = "Budgets",     ActionTime = DateTime.Now.AddDays(-3),    Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "CREATE User 'organizer2' (ID=3)",    TableName = "Users",       ActionTime = DateTime.Now.AddDays(-15),   Status = "Recorded" },
                new SystemAuditLog { UserId = userAdmin.UserId, Action = "UPDATE Department 'Ban Marketing'",  TableName = "Departments", ActionTime = DateTime.Now.AddDays(-20),   Status = "Recorded" },
            });
            await db.SaveChangesAsync();

            Console.WriteLine("✅ Seed data hoàn thành!");
            Console.WriteLine("   Login: admin / Admin@123");
            Console.WriteLine("   Budget Pending: 3 | Approved: 6 | Rejected: 1");
            Console.WriteLine($"  Tickets: {tickets.Count} vé (80% Paid)");
            Console.WriteLine($"  Equipments: {equipments.Count} thiết bị");
            Console.WriteLine($"  Surveys: {surveys.Count} đánh giá");
            Console.WriteLine($"  FieldReports: {fieldReports.Count} báo cáo");
        }
    }
}
