// Repositories/WineRepository.cs
using BroxDistribution.Models;
using BroxDistribution1.Repositories;

namespace BroxDistribution.Repositories
{
    public class WineRepository : JsonFileRepository<Wine>
    {
        public WineRepository(IWebHostEnvironment environment) 
            : base(environment, "wines.json")
        {
        }

        public async Task<IEnumerable<Wine>> GetActiveWinesAsync()
        {
            var wines = await GetAllAsync();
            return wines.Where(w => !w.IsDeleted);
        }

        public async Task<IEnumerable<Wine>> GetByBrandAsync(string brand)
        {
            var wines = await GetAllAsync();
            return wines.Where(w => !w.IsDeleted && w.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Wine>> GetByCategoryAsync(string category)
        {
            var wines = await GetAllAsync();
            return wines.Where(w => !w.IsDeleted && w.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Wine>> SearchWinesAsync(string search)
        {
            var wines = await GetAllAsync();
            if (string.IsNullOrEmpty(search))
                return wines.Where(w => !w.IsDeleted);

            return wines.Where(w => !w.IsDeleted && (
                w.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                w.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                w.Category.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                w.Country.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                w.Region.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                w.Grape.Contains(search, StringComparison.OrdinalIgnoreCase)
            ));
        }
    }
}