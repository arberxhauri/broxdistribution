// Repositories/AdminRepository.cs
using BroxDistribution1.Models;

namespace BroxDistribution1.Repositories
{
    public class AdminRepository : JsonFileRepository<Admin>
    {
        public AdminRepository(IWebHostEnvironment environment)
            : base(environment, "admins.json")
        {
        }

        public async Task<Admin?> GetByUsernameAsync(string username)
        {
            var admins = await GetAllAsync();
            return admins.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Admin?> GetByEmailAsync(string email)
        {
            var admins = await GetAllAsync();
            return admins.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}