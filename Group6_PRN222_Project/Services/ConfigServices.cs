using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Services
{
    // ══════════════════════════════════════════════════════════════
    // ROLE SERVICE
    // ══════════════════════════════════════════════════════════════
    public interface IRoleService
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?>      GetByIdAsync(int id);
        Task             UpdateStatusAsync(int id, string status);
    }

    public class RoleService : IRoleService
    {
        private readonly ProjectPrn222Context _db;
        public RoleService(ProjectPrn222Context db) => _db = db;

        public Task<List<Role>> GetAllAsync()
            => _db.Roles.OrderBy(r => r.RoleId).ToListAsync();

        public Task<Role?> GetByIdAsync(int id)
            => _db.Roles.FirstOrDefaultAsync(r => r.RoleId == id);

        public async Task UpdateStatusAsync(int id, string status)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return;
            role.Status = status;
            await _db.SaveChangesAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // DISCOUNT POLICY SERVICE
    // ══════════════════════════════════════════════════════════════
    public interface IDiscountPolicyService
    {
        Task<List<DiscountPolicy>> GetAllAsync(int? eventId = null);
        Task<DiscountPolicy?>      GetByIdAsync(int id);
        Task<DiscountPolicy>       CreateAsync(DiscountPolicy policy);
        Task                       UpdateAsync(DiscountPolicy policy);
        Task                       DeleteAsync(int id);
    }

    public class DiscountPolicyService : IDiscountPolicyService
    {
        private readonly ProjectPrn222Context _db;
        public DiscountPolicyService(ProjectPrn222Context db) => _db = db;

        public Task<List<DiscountPolicy>> GetAllAsync(int? eventId = null)
        {
            var q = _db.DiscountPolicies.Include(p => p.Event).AsQueryable();
            if (eventId.HasValue) q = q.Where(p => p.EventId == eventId);
            return q.OrderByDescending(p => p.PolicyId).ToListAsync();
        }

        public Task<DiscountPolicy?> GetByIdAsync(int id)
            => _db.DiscountPolicies.Include(p => p.Event)
                  .FirstOrDefaultAsync(p => p.PolicyId == id);

        public async Task<DiscountPolicy> CreateAsync(DiscountPolicy policy)
        {
            _db.DiscountPolicies.Add(policy);
            await _db.SaveChangesAsync();
            return policy;
        }

        public async Task UpdateAsync(DiscountPolicy policy)
        {
            _db.DiscountPolicies.Update(policy);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.DiscountPolicies.FindAsync(id);
            if (p != null) { _db.DiscountPolicies.Remove(p); await _db.SaveChangesAsync(); }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // EQUIPMENT SERVICE
    // ══════════════════════════════════════════════════════════════
    public interface IEquipmentService
    {
        Task<List<Equipment>> GetAllAsync(string? statusFilter = null);
        Task<Equipment?>      GetByIdAsync(int id);
        Task<Equipment>       CreateAsync(Equipment equipment);
        Task                  UpdateAsync(Equipment equipment);
        Task                  DeleteAsync(int id);
    }

    public class EquipmentService : IEquipmentService
    {
        private readonly ProjectPrn222Context _db;
        public EquipmentService(ProjectPrn222Context db) => _db = db;

        public Task<List<Equipment>> GetAllAsync(string? statusFilter = null)
        {
            var q = _db.Equipments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                q = q.Where(e => e.Status == statusFilter);
            return q.OrderBy(e => e.EquipmentId).ToListAsync();
        }

        public Task<Equipment?> GetByIdAsync(int id)
            => _db.Equipments.FirstOrDefaultAsync(e => e.EquipmentId == id);

        public async Task<Equipment> CreateAsync(Equipment equipment)
        {
            _db.Equipments.Add(equipment);
            await _db.SaveChangesAsync();
            return equipment;
        }

        public async Task UpdateAsync(Equipment equipment)
        {
            _db.Equipments.Update(equipment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Equipments.FindAsync(id);
            if (e != null) { _db.Equipments.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}
