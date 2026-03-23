using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ─── DTOs ─────────────────────────────────────────────────────
    public class BudgetListItemDto
    {
        public int     BudgetId        { get; set; }
        public int     EventId         { get; set; }
        public string  EventName       { get; set; } = "";
        public string? OrganizerName   { get; set; }
        public decimal TotalAllocated  { get; set; }
        public decimal SpentAmount     { get; set; }
        public decimal Remaining       => TotalAllocated - SpentAmount;
        public string  ApprovalStatus  { get; set; } = "Pending";
        public string? ApprovedByName  { get; set; }
        public DateTime? EventStartDate { get; set; }
    }

    // ─── Interface ────────────────────────────────────────────────
    public interface IBudgetService
    {
        Task<List<BudgetListItemDto>> GetAllAsync(string? statusFilter = null);
        Task<BudgetListItemDto?>      GetByIdAsync(int budgetId);
        Task<List<BudgetListItemDto>> GetByEventIdAsync(int eventId);
        Task<bool> ApproveAsync(int budgetId, int approvedByUserId);
        Task<bool> RejectAsync(int budgetId,  int approvedByUserId);
    }

    // ─── Implementation ───────────────────────────────────────────
    public class BudgetService : IBudgetService
    {
        private readonly ProjectPrn222Context _db;

        public BudgetService(ProjectPrn222Context db)
        {
            _db = db;
        }

        public async Task<List<BudgetListItemDto>> GetAllAsync(string? statusFilter = null)
        {
            var query = _db.Budgets
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Organizer)
                .Include(b => b.ApprovedByNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                query = query.Where(b => b.ApprovalStatus == statusFilter);

            return await query
                .OrderByDescending(b => b.BudgetId)
                .Select(b => new BudgetListItemDto
                {
                    BudgetId       = b.BudgetId,
                    EventId        = b.EventId ?? 0,
                    EventName      = b.Event != null ? b.Event.EventName : "—",
                    OrganizerName  = b.Event != null && b.Event.Organizer != null
                                     ? (b.Event.Organizer.FullName ?? b.Event.Organizer.Username)
                                     : null,
                    TotalAllocated = b.TotalAllocated ?? 0,
                    SpentAmount    = b.SpentAmount    ?? 0,
                    ApprovalStatus = b.ApprovalStatus ?? "Pending",
                    ApprovedByName = b.ApprovedByNavigation != null
                                     ? (b.ApprovedByNavigation.FullName ?? b.ApprovedByNavigation.Username)
                                     : null,
                    EventStartDate = b.Event != null ? b.Event.StartDate : null
                })
                .ToListAsync();
        }

        public async Task<BudgetListItemDto?> GetByIdAsync(int budgetId)
        {
            var b = await _db.Budgets
                .Include(x => x.Event).ThenInclude(e => e!.Organizer)
                .Include(x => x.ApprovedByNavigation)
                .FirstOrDefaultAsync(x => x.BudgetId == budgetId);

            if (b == null) return null;

            return new BudgetListItemDto
            {
                BudgetId       = b.BudgetId,
                EventId        = b.EventId ?? 0,
                EventName      = b.Event?.EventName ?? "—",
                OrganizerName  = b.Event?.Organizer?.FullName ?? b.Event?.Organizer?.Username,
                TotalAllocated = b.TotalAllocated ?? 0,
                SpentAmount    = b.SpentAmount    ?? 0,
                ApprovalStatus = b.ApprovalStatus ?? "Pending",
                ApprovedByName = b.ApprovedByNavigation?.FullName ?? b.ApprovedByNavigation?.Username,
                EventStartDate = b.Event?.StartDate
            };
        }

        public async Task<List<BudgetListItemDto>> GetByEventIdAsync(int eventId)
            => await GetAllAsync().ContinueWith(t =>
                t.Result.Where(b => b.EventId == eventId).ToList());

        public async Task<bool> ApproveAsync(int budgetId, int approvedByUserId)
        {
            var budget = await _db.Budgets.FindAsync(budgetId);
            if (budget == null) return false;

            budget.ApprovalStatus = "Approved";
            budget.ApprovedBy     = approvedByUserId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int budgetId, int approvedByUserId)
        {
            var budget = await _db.Budgets.FindAsync(budgetId);
            if (budget == null) return false;

            budget.ApprovalStatus = "Rejected";
            budget.ApprovedBy     = approvedByUserId;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
