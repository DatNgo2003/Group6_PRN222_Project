using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Project_PRN222.Helpers;

namespace Project_PRN222.Services
{
    // ─── Interface ────────────────────────────────────────────────
    public interface IUserService
    {
        Task<List<User>>       GetAllAsync();
        Task<User?>            GetByIdAsync(int id);
        Task<bool>             UsernameExistsAsync(string username, int? excludeId = null);
        Task<User>             CreateAsync(User user, string plainPassword);
        Task                   UpdateAsync(User user, string? newPlainPassword);
        Task                   DeleteAsync(int id);
    }

    // ─── Implementation ───────────────────────────────────────────
    public class UserService : IUserService
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService     _audit;

        public UserService(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db    = db;
            _audit = audit;
        }

        public Task<List<User>> GetAllAsync()
            => _db.Users
                  .Include(u => u.Role)
                  .Include(u => u.Department)
                  .OrderBy(u => u.UserId)
                  .ToListAsync();

        public Task<User?> GetByIdAsync(int id)
            => _db.Users
                  .Include(u => u.Role)
                  .Include(u => u.Department)
                  .FirstOrDefaultAsync(u => u.UserId == id);

        public Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
            => _db.Users.AnyAsync(u =>
                   u.Username == username &&
                   (excludeId == null || u.UserId != excludeId));

        public async Task<User> CreateAsync(User user, string plainPassword)
        {
            user.PasswordHash = PasswordHasher.Hash(plainPassword);
            user.CreatedAt    = DateTime.Now;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user, string? newPlainPassword)
        {
            if (!string.IsNullOrWhiteSpace(newPlainPassword))
                user.PasswordHash = PasswordHasher.Hash(newPlainPassword);

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }
    }
}
