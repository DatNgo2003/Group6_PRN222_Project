using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── Interface ────────────────────────────────────────────────
    public interface IAuditLogService
    {
        void Log(int actorUserID, string action, string tableName);
    }

    // ─── Implementation ───────────────────────────────────────────
    public class AuditLogService : IAuditLogService
    {
        private readonly ProjectPrn222Context _db;

        public AuditLogService(ProjectPrn222Context db)
        {
            _db = db;
        }

        public void Log(int actorUserID, string action, string tableName)
        {
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId     = actorUserID,
                Action     = action,
                TableName  = tableName,
                ActionTime = DateTime.Now
            });
            _db.SaveChanges();
        }
    }
}
