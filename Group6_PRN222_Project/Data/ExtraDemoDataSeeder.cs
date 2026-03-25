using Group6_PRN222_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Data;

/// <summary>
/// Bổ sung participant + vé + vài báo cáo Security cho DB đã có sự kiện (không phụ thuộc seed ban đầu).
/// Chỉ chạy một lần; chạy lại: xóa dòng audit Action = <see cref="MarkerAction"/>.
/// </summary>
public static class ExtraDemoDataSeeder
{
    public const string MarkerAction = "SEED_EXTRA_DEMO_V1";

    public static async System.Threading.Tasks.Task EnsureAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectPrn222Context>();

        await db.Database.EnsureCreatedAsync();

        if (await db.SystemAuditLogs.AsNoTracking().AnyAsync(l => l.Action == MarkerAction))
            return;

        var events = await db.Events.AsNoTracking().OrderBy(e => e.EventId).ToListAsync();
        if (events.Count == 0)
            return;

        var securityUser = await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Role != null &&
                (u.Role.RoleName == "Staff(Security)" ||
                 u.Role.RoleName == "Staff" ||
                 u.Role.RoleName == "Security"));

        int? securityId = securityUser?.UserId;

        const int bulkParticipantCount = 45;
        var participants = new List<Participant>();
        for (var i = 0; i < bulkParticipantCount; i++)
        {
            var isVip = i % 6 == 0;
            var isBl = i % 13 == 0 && i > 2;
            participants.Add(new Participant
            {
                FullName = $"Khách demo {i + 1:000}",
                Email = $"demo.bulk.{i + 1:000}@eventpro.bulk",
                Phone = "090" + (1_000_000 + i).ToString("D7"),
                IsVip = isVip,
                IsBlacklisted = isBl,
                Status = "Active"
            });
        }

        db.Participants.AddRange(participants);
        await db.SaveChangesAsync();

        var rnd = new Random(20250325);
        var payCycle = new[] { "Paid", "Paid", "Paid", "Paid", "Pending" };
        var tickets = new List<Ticket>();
        var pIx = 0;

        foreach (var ev in events)
        {
            var n = 10 + rnd.Next(18);
            for (var t = 0; t < n; t++)
            {
                var paid = payCycle[(t + ev.EventId) % payCycle.Length];
                DateTime? checkIn = null;
                if (string.Equals(paid, "Paid", StringComparison.Ordinal) && rnd.Next(4) != 0)
                    checkIn = DateTime.Now.AddHours(-rnd.Next(1, 120));

                tickets.Add(new Ticket
                {
                    EventId = ev.EventId,
                    ParticipantId = participants[pIx % participants.Count].ParticipantId,
                    Qrcode = Guid.NewGuid(),
                    PaymentStatus = paid,
                    CheckInTime = checkIn,
                    Status = "Valid"
                });
                pIx++;
            }
        }

        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        if (securityId.HasValue)
        {
            var reports = events.Take(Math.Min(6, events.Count)).Select((ev, i) => new FieldReport
            {
                EventId = ev.EventId,
                StaffId = securityId,
                ReportType = "Security",
                Content = "[Demo tự động] Tuần tra cổng — luồng khách ổn, không sự cố.",
                ReportTime = DateTime.Now.AddHours(-(i + 1) * 4),
                Status = "Submitted"
            }).ToList();
            db.FieldReports.AddRange(reports);
        }

        var anyUserId = await db.Users.AsNoTracking().Select(u => (int?)u.UserId).FirstOrDefaultAsync();
        db.SystemAuditLogs.Add(new SystemAuditLog
        {
            UserId = anyUserId,
            Action = MarkerAction,
            TableName = "ALL",
            ActionTime = DateTime.Now,
            Status = "Recorded"
        });

        await db.SaveChangesAsync();

        Console.WriteLine($"✅ Extra demo data: +{participants.Count} participants, +{tickets.Count} tickets.");
    }
}
