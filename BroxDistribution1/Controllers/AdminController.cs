// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BroxDistribution1.ViewModel;
using BroxDistribution1.Services;
using System.Security.Claims;
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using BroxDistribution1.Repositories;
using Microsoft.AspNetCore.Hosting;

namespace BroxDistribution.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepository;
        private readonly WineRepository _wineRepository;
        private readonly ContactRepository _contactRepository;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            AdminRepository adminRepository,
            WineRepository wineRepository,
            ContactRepository contactRepository,
            IEmailService emailService,
            IWebHostEnvironment env)
        {
            _adminRepository = adminRepository;
            _wineRepository = wineRepository;
            _contactRepository = contactRepository;
            _emailService = emailService;
            _env = env;
        }

        // -------------------- Public image endpoints (App_Data) --------------------

        // Wine images: /media/wines/{file}
        [AllowAnonymous]
        [HttpGet("/media/wines/{file}")]
        public IActionResult WineImage(string file)
        {
            file = SafeFileName(file);
            if (string.IsNullOrWhiteSpace(file)) return NotFound();

            var path = Path.Combine(_env.ContentRootPath, "App_Data", "images", "wines", file);
            if (!System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, GetMimeType(path));
        }

        // General images if you want (banner etc) in App_Data/images: /media/{file}
        [AllowAnonymous]
        [HttpGet("/media/{file}")]
        public IActionResult SiteImage(string file)
        {
            file = SafeFileName(file);
            if (string.IsNullOrWhiteSpace(file)) return NotFound();

            var path = Path.Combine(_env.ContentRootPath, "App_Data", "images", file);
            if (!System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, GetMimeType(path));
        }

        // GET: Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = await _adminRepository.GetByUsernameAsync(model.Username);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            admin.LastLoginAt = DateTime.Now;
            await _adminRepository.UpdateAsync(admin);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Admin/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: Admin/Dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var allWines = await _wineRepository.GetAllAsync();
            var activeWines = await _wineRepository.GetActiveWinesAsync();
            var allContacts = await _contactRepository.GetAllAsync();

            var stats = new
            {
                TotalWines = allWines.Count(),
                ActiveWines = activeWines.Count(),
                DeletedWines = allWines.Count(w => w.IsDeleted),
                TotalContacts = allContacts.Count(),
                UnreadContacts = allContacts.Count(c => !c.IsRead),
                RepliedContacts = allContacts.Count(c => c.IsReplied)
            };

            return View(stats);
        }

        // ==================== WINE MANAGEMENT ====================

        // GET: Admin/Wines
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Wines(
            bool showDeleted = false,
            string sortBy = "Name",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10,
            string search = "",
            string category = "")
        {
            var wines = await _wineRepository.GetAllAsync();

            wines = showDeleted
                ? wines.Where(w => w.IsDeleted)
                : wines.Where(w => !w.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                wines = wines.Where(w =>
                    (w.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Brand ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (w.Description ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category))
                wines = wines.Where(w => w.Category == category);

            wines = sortBy switch
            {
                "Name" => sortOrder == "asc" ? wines.OrderBy(w => w.Name) : wines.OrderByDescending(w => w.Name),
                "Brand" => sortOrder == "asc" ? wines.OrderBy(w => w.Brand) : wines.OrderByDescending(w => w.Brand),
                "Category" => sortOrder == "asc" ? wines.OrderBy(w => w.Category) : wines.OrderByDescending(w => w.Category),
                "Country" => sortOrder == "asc" ? wines.OrderBy(w => w.Country) : wines.OrderByDescending(w => w.Country),
                "Year" => sortOrder == "asc" ? wines.OrderBy(w => w.Year) : wines.OrderByDescending(w => w.Year),
                "Price" => sortOrder == "asc" ? wines.OrderBy(w => w.Price) : wines.OrderByDescending(w => w.Price),
                _ => wines.OrderBy(w => w.Name)
            };

            var totalItems = wines.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var paginatedWines = wines
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var allWinesForCategories = await _wineRepository.GetAllAsync();
            var categories = allWinesForCategories
                .Where(w => !string.IsNullOrEmpty(w.Category))
                .Select(w => w.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.ShowDeleted = showDeleted;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Categories = categories;

            return View(paginatedWines);
        }

        // GET: Admin/CreateWine
        [Authorize(Roles = "Admin")]
        public IActionResult CreateWine() => View();

        // POST: Admin/CreateWine
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWine(Wine wine, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(wine);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var imageUrl = await SaveWineImageAsync(ImageFile);
                if (imageUrl != null)
                    wine.ImageUrl = imageUrl;
            }

            wine.ImageUrl ??= "/images/wines/default-wine.jpg"; // default stays in wwwroot

            await _wineRepository.AddAsync(wine);
            TempData["Success"] = "Wine added successfully!";
            return RedirectToAction(nameof(Wines));
        }

        // GET: Admin/EditWine/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine == null) return NotFound();
            return View(wine);
        }

        // POST: Admin/EditWine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id, Wine wine, IFormFile? ImageFile)
        {
            if (id != wine.Id) return NotFound();
            if (!ModelState.IsValid) return View(wine);

            // Load existing so you don't lose ImageUrl if the form doesn't post it
            var existing = await _wineRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Update fields (copy what you allow editing)
            existing.Name = wine.Name;
            existing.Brand = wine.Brand;
            existing.Category = wine.Category;
            existing.Country = wine.Country;
            existing.Region = wine.Region;
            existing.Grape = wine.Grape;
            existing.Year = wine.Year;
            existing.Price = wine.Price;
            existing.Description = wine.Description;
            existing.IsDeleted = wine.IsDeleted; // optional; or keep your delete/restore only
            existing.DeletedAt = wine.DeletedAt;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                // delete old App_Data image if it was one of ours
                DeleteWineImageIfOwned(existing.ImageUrl);

                var imageUrl = await SaveWineImageAsync(ImageFile);
                if (imageUrl != null)
                    existing.ImageUrl = imageUrl;
            }

            existing.ImageUrl ??= "/images/wines/default-wine.jpg";

            await _wineRepository.UpdateAsync(existing);
            TempData["Success"] = "Wine updated successfully!";
            return RedirectToAction(nameof(Wines));
        }

        // POST: Admin/DeleteWine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWine(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine != null)
            {
                wine.IsDeleted = true;
                wine.DeletedAt = DateTime.Now;
                await _wineRepository.UpdateAsync(wine);
                TempData["Success"] = "Wine deleted successfully!";
            }
            return RedirectToAction(nameof(Wines));
        }

        // POST: Admin/RestoreWine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreWine(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine != null && wine.IsDeleted)
            {
                wine.IsDeleted = false;
                wine.DeletedAt = null;
                await _wineRepository.UpdateAsync(wine);
                TempData["Success"] = "Wine restored successfully!";
            }
            return RedirectToAction(nameof(Wines), new { showDeleted = true });
        }

        // ==================== CONTACT FORM MANAGEMENT ====================

        // GET: Admin/Contacts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Contacts()
        {
            var contacts = (await _contactRepository.GetAllAsync())
                .OrderByDescending(c => c.SubmittedAt)
                .ToList();
            return View(contacts);
        }

        // GET: Admin/ContactDetails/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ContactDetails(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            if (contact == null) return NotFound();

            if (!contact.IsRead)
                await _contactRepository.MarkAsReadAsync(id);

            return View(contact);
        }

        // POST: Admin/ReplyContact/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyContact(int id, string replyMessage)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            if (contact == null) return NotFound();

            if (string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["Error"] = "Reply message cannot be empty!";
                return RedirectToAction(nameof(ContactDetails), new { id });
            }

            try
            {
                var subject = "Re: Your inquiry - Brox Distribution";

                string safeReply = WebUtility.HtmlEncode(replyMessage).Replace("\n", "<br>");
                string safeOriginal = WebUtility.HtmlEncode(contact.Description ?? "").Replace("\n", "<br>");

                var emailBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
  <h2 style='color: #1c1c1e;'>Hello {WebUtility.HtmlEncode(contact.Name)},</h2>
  <p>Thank you for contacting Brox Distribution. Here's our response to your inquiry:</p>
  <div style='background: #faf8f5; padding: 20px; border-left: 4px solid #FF5722; margin: 20px 0;'>
    {safeReply}
  </div>
  <p><strong>Your original message:</strong></p>
  <p style='color: #666;'>{safeOriginal}</p>
  <hr style='border: none; border-top: 1px solid #e8e6e3; margin: 20px 0;'>
  <p style='color: #999; font-size: 12px;'>
    Best regards,<br>
    Brox Distribution Team<br>
    <a href='mailto:info@broxdistribution.co.uk'>info@broxdistribution.co.uk</a>
  </p>
</body>
</html>";

                await _emailService.SendEmailAsync(contact.Email, subject, emailBody);

                contact.IsReplied = true;
                contact.RepliedAt = DateTime.Now;
                contact.ReplyMessage = replyMessage;
                await _contactRepository.UpdateAsync(contact);

                TempData["Success"] = "Reply sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to send email: {ex.Message}";
            }

            return RedirectToAction(nameof(ContactDetails), new { id });
        }

        // POST: Admin/DeleteContact/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            await _contactRepository.DeleteAsync(id);
            TempData["Success"] = "Contact deleted successfully!";
            return RedirectToAction(nameof(Contacts));
        }

        // -------------------- Image helpers (App_Data) --------------------

        private async Task<string?> SaveWineImageAsync(IFormFile imageFile)
        {
            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Invalid image format. Allowed: JPG, PNG, WEBP, GIF";
                    return null;
                }

                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Image size must be less than 5MB";
                    return null;
                }

                var fileName = $"{Guid.NewGuid()}{extension}";

                var uploadsFolder = Path.Combine(_env.ContentRootPath, "App_Data", "images", "wines");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // IMPORTANT: Return the controller URL, not /images/...
                return $"/media/wines/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                TempData["Error"] = "Failed to save image";
                return null;
            }
        }

        private void DeleteWineImageIfOwned(string? imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl)) return;

                // Only delete images stored in App_Data via our /media/wines route
                // Example: /media/wines/abc.jpg
                if (!imageUrl.StartsWith("/media/wines/", StringComparison.OrdinalIgnoreCase))
                    return;

                var fileName = Path.GetFileName(imageUrl);
                fileName = SafeFileName(fileName);
                if (string.IsNullOrWhiteSpace(fileName)) return;

                var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "images", "wines", fileName);

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }

        private static string SafeFileName(string input)
        {
            // Prevent path traversal like ../../etc/passwd
            return Path.GetFileName(input ?? "").Trim();
        }

        private static string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
