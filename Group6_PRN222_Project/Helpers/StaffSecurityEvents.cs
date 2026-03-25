using Group6_PRN222_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Helpers;

public static class StaffSecurityEvents
{
    /// <summary>
    /// Ưu tiên sự kiện có task gán cho nhân viên; nếu không có thì lấy các sự kiện gần đây.
    /// </summary>
    public static async System.Threading.Tasks.Task<List<Event>> LoadEventsForStaffAsync(ProjectPrn222Context db, int userId)
    {
        var eventIds = await db.Tasks.AsNoTracking()
            .Where(t => t.AssignedTo == userId && t.EventId != null)
            .Select(t => t.EventId!.Value)
            .Distinct()
            .ToListAsync();

        IQueryable<Event> q = db.Events.AsNoTracking();
        if (eventIds.Count > 0)
            q = q.Where(e => eventIds.Contains(e.EventId));

        var list = await q.OrderBy(e => e.StartDate).ToListAsync();
        if (list.Count == 0)
        {
            list = await db.Events.AsNoTracking()
                .OrderByDescending(e => e.StartDate)
                .Take(24)
                .ToListAsync();
        }

        return list;
    }
}
