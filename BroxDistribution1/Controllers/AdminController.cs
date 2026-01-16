// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BroxDistribution1.Models;
using BroxDistribution1.ViewModel;
using BroxDistribution1.Services;
using System.Security.Claims;
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BroxDistribution1.Repositories;
using System.Net;


namespace BroxDistribution.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepository;
        private readonly WineRepository _wineRepository;
        private readonly ContactRepository _contactRepository;
        private readonly IEmailService _emailService;

        public AdminController(
            AdminRepository adminRepository,
            WineRepository wineRepository,
            ContactRepository contactRepository,
            IEmailService emailService)
        {
            _adminRepository = adminRepository;
            _wineRepository = wineRepository;
            _contactRepository = contactRepository;
            _emailService = emailService;
        }

        // GET: Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admin = await _adminRepository.GetByUsernameAsync(model.Username);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Update last login
            admin.LastLoginAt = DateTime.Now;
            await _adminRepository.UpdateAsync(admin);

            // Create claims
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
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
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

            // Filter by deleted status
            if (showDeleted)
            {
                wines = wines.Where(w => w.IsDeleted);
            }
            else
            {
                wines = wines.Where(w => !w.IsDeleted);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                wines = wines.Where(w =>
                    w.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                wines = wines.Where(w => w.Category == category);
            }

            // Sorting
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

            // Get total count for pagination
            var totalItems = wines.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // Get paginated results
            var paginatedWines = wines
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct categories for filter dropdown
            var allWinesForCategories = await _wineRepository.GetAllAsync();
            var categories = allWinesForCategories
                .Where(w => !string.IsNullOrEmpty(w.Category))
                .Select(w => w.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Pass data to view
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
        public IActionResult CreateWine()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWine(Wine wine, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(ImageFile);
                    if (imageUrl != null)
                    {
                        wine.ImageUrl = imageUrl;
                    }
                }
                else
                {
                    // Set default image if no image uploaded
                    wine.ImageUrl = "/images/wines/default-wine.jpg";
                }

                await _wineRepository.AddAsync(wine);
                TempData["Success"] = "Wine added successfully!";
                return RedirectToAction(nameof(Wines));
            }
            return View(wine);
        }

        // GET: Admin/EditWine/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id)
        {
            var wine = await _wineRepository.GetByIdAsync(id);
            if (wine == null)
            {
                return NotFound();
            }
            return View(wine);
        }

        // Update EditWine method
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id, Wine wine, IFormFile ImageFile)
        {
            if (id != wine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Handle image upload if new image provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Delete old image if it exists and is not the default
                    if (!string.IsNullOrEmpty(wine.ImageUrl) && 
                        wine.ImageUrl != "/images/wines/default-wine.jpg")
                    {
                        DeleteImage(wine.ImageUrl);
                    }

                    var imageUrl = await SaveImageAsync(ImageFile);
                    if (imageUrl != null)
                    {
                        wine.ImageUrl = imageUrl;
                    }
                }

                await _wineRepository.UpdateAsync(wine);
                TempData["Success"] = "Wine updated successfully!";
                return RedirectToAction(nameof(Wines));
            }
            return View(wine);
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
            if (contact == null)
            {
                return NotFound();
            }

            // Mark as read
            if (!contact.IsRead)
            {
                await _contactRepository.MarkAsReadAsync(id);
            }

            return View(contact);
        }

        // POST: Admin/ReplyContact/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyContact(int id, string replyMessage)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["Error"] = "Reply message cannot be empty!";
                return RedirectToAction(nameof(ContactDetails), new { id });
            }

            try
            {
                // Send email
                var subject = $"Re: Your inquiry - Brox Distribution";
                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #1c1c1e;'>Hello {contact.Name},</h2>
                        <p>Thank you for contacting Brox Distribution. Here's our response to your inquiry:</p>
                        <div style='background: #faf8f5; padding: 20px; border-left: 4px solid #FF5722; margin: 20px 0;'>
                            {WebUtility.HtmlEncode(replyMessage).Replace("\n", "<br>")}
                        </div>
                        <p><strong>Your original message:</strong></p>
                        <p style='color: #666;'>{contact.Description}</p>
                        <hr style='border: none; border-top: 1px solid #e8e6e3; margin: 20px 0;'>
                        <p style='color: #999; font-size: 12px;'>
                            Best regards,<br>
                            Brox Distribution Team<br>
                            <a href='mailto:info@broxdistribution.com'>info@broxdistribution.com</a>
                        </p>
                    </body>
                    </html>
                ";

                await _emailService.SendEmailAsync(contact.Email, subject, emailBody);

                // Update contact form
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
        
        private async Task<string> SaveImageAsync(IFormFile imageFile)
{
    try
    {
        // Validate file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            TempData["Error"] = "Invalid image format. Allowed: JPG, PNG, WEBP, GIF";
            return null;
        }

        if (imageFile.Length > 5 * 1024 * 1024) // 5MB limit
        {
            TempData["Error"] = "Image size must be less than 5MB";
            return null;
        }

        // Create unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "images", "wines");
        
        // Ensure directory exists
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(fileStream);
        }

        // Return relative URL
        return $"/images/wines/{fileName}";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving image: {ex.Message}");
        TempData["Error"] = "Failed to save image";
        return null;
    }
}

        private void DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/wines/default-wine.jpg")
                    return;

                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "images", "wines", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"Deleted image: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}
