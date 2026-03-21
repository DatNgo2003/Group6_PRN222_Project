using System.Security.Cryptography;
using System.Text;

namespace Project_PRN222.Helpers
{
    public static class PasswordHasher
    {
        // SHA-256 hash — đủ dùng cho project PRN222
        // Production nên dùng BCrypt / PBKDF2
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static bool Verify(string password, string hash)
            => Hash(password) == hash;
    }
}
