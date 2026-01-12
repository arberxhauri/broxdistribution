// Controllers/ProductsController.cs
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using BroxDistribution1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BroxDistribution.Controllers
{
    public class ProductsController : Controller
    {
        private readonly WineRepository _wineRepository;

        public ProductsController(WineRepository wineRepository)
        {
            _wineRepository = wineRepository;
        }

        public async Task<IActionResult> Index(string search)
        {
            var wines = await _wineRepository.GetActiveWinesAsync();

            if (!string.IsNullOrEmpty(search))
            {
                wines = wines.Where(w =>
                    w.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Category.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Country.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Region.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Grape.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            return View(wines.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine == null || wine.IsDeleted)
                return NotFound();

            return View(wine);
        }
    }
}