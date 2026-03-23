using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
namespace Project_PRN222.Services
{
    // ─── Interface ────────────────────────────────────────────────
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?>      GetByIdAsync(int id);
        Task<bool>             NameExistsAsync(string name, int? excludeId = null);
        Task<Department>       CreateAsync(Department dept);
        Task                   UpdateAsync(Department dept);
        Task<bool>             DeleteAsync(int id);  // false nếu có user đang dùng
    }

    // ─── Implementation ───────────────────────────────────────────
    public class DepartmentService : IDepartmentService
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService     _audit;

        public DepartmentService(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db    = db;
            _audit = audit;
        }

        public Task<List<Department>> GetAllAsync()
            => _db.Departments
                  .OrderBy(d => d.DepartmentId)
                  .ToListAsync();

        public Task<Department?> GetByIdAsync(int id)
            => _db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);

        public Task<bool> NameExistsAsync(string name, int? excludeId = null)
            => _db.Departments.AnyAsync(d =>
                   d.DepartmentName == name &&
                   (excludeId == null || d.DepartmentId != excludeId));

        public async Task<Department> CreateAsync(Department dept)
        {
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();
            return dept;
        }

        public async Task UpdateAsync(Department dept)
        {
            _db.Departments.Update(dept);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Kiểm tra có user nào thuộc department này không
            bool hasUsers = await _db.Users.AnyAsync(u => u.DepartmentId == id);
            if (hasUsers) return false;

            var dept = await _db.Departments.FindAsync(id);
            if (dept == null) return false;

            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
