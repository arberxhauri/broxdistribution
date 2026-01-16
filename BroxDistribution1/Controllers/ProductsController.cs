// Controllers/ProductsController.cs
using BroxDistribution.Models;
using BroxDistribution.Repositories;
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

        public async Task<IActionResult> Index(string? search)
        {
            var wines = await _wineRepository.GetActiveWinesAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                wines = wines.Where(w =>
                    (w.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Brand ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Category ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Country ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Region ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Grape ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            ViewData["Title"] = "Wine Portfolio | Premium Wine Collection | Brox Distribution";
            ViewData["MetaDescription"] =
                "Browse our curated wine portfolio featuring premium selections from top regions and producers. Filter wines by name, grape, country or category.";
            ViewData["OgTitle"] = "Brox Distribution - Wine Portfolio";
            ViewData["OgDescription"] =
                "Discover exceptional wines from our premium portfolio for HoReCa and retail partners.";
            ViewData["OgType"] = "website";

            return View(wines.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine == null || wine.IsDeleted)
                return NotFound();

            var regionOrCountry = !string.IsNullOrEmpty(wine.Region) ? wine.Region : wine.Country;

            ViewData["Title"] = $"{wine.Name} {(wine.Year > 0 ? wine.Year.ToString() : "")} | {regionOrCountry} Wine | Brox Distribution";
            ViewData["MetaDescription"] =
                $"{wine.Name} by {wine.Brand} – {wine.Category} wine from {regionOrCountry}. {(!string.IsNullOrEmpty(wine.Description) ? wine.Description[..Math.Min(150, wine.Description.Length)] : "Discover this premium wine.")}";
            ViewData["OgTitle"] = $"{wine.Name} | {wine.Brand}";
            ViewData["OgDescription"] =
                $"{wine.Category} wine from {regionOrCountry}. {wine.Brand} – {wine.Year} vintage.";
            ViewData["OgType"] = "product";
            ViewData["OgImage"] = wine.ImageUrl; // will be /media/wines/... for uploads

            return View(wine);
        }
    }
}
