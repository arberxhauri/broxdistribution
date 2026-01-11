using System.Linq;
using BroxDistribution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroxDistribution.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var wines = from w in _context.Wines
                select w;

            if (!string.IsNullOrEmpty(search))
            {
                wines = wines.Where(w => 
                    w.Name.Contains(search) ||
                    w.Brand.Contains(search) ||
                    w.Category.Contains(search) ||
                    w.Country.Contains(search) ||
                    w.Region.Contains(search) ||
                    w.Grape.Contains(search)
                );
            }

            return View(await wines.ToListAsync());
        }


        public IActionResult Details(int id)
        {
            var wine = _context.Wines.Find(id);
            if (wine == null)
                return NotFound();

            return View(wine);
        }
    }
}