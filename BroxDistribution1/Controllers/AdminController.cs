// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BroxDistribution1.Models;
using BroxDistribution1.ViewModel;
using BroxDistribution1.Services;
using System.Security.Claims;
using BroxDistribution.Models;
using BroxDistribution1.Services;
using BroxDistribution1.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BroxDistribution.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
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

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Update last login
            admin.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

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
            var stats = new
            {
                TotalWines = await _context.Wines.CountAsync(),
                ActiveWines = await _context.Wines.Where(w => !w.IsDeleted).CountAsync(),
                DeletedWines = await _context.Wines.IgnoreQueryFilters().CountAsync(w => w.IsDeleted),
                TotalContacts = await _context.ContactForms.CountAsync(),
                UnreadContacts = await _context.ContactForms.CountAsync(c => !c.IsRead),
                RepliedContacts = await _context.ContactForms.CountAsync(c => c.IsReplied)
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
    var query = _context.Wines.AsQueryable();

    if (showDeleted)
    {
        query = query.IgnoreQueryFilters().Where(w => w.IsDeleted);
    }

    // Search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(w => 
            w.Name.Contains(search) || 
            w.Brand.Contains(search) ||
            w.Description.Contains(search));
    }

    // Category filter
    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(w => w.Category == category);
    }

    // Sorting
    query = sortBy switch
    {
        "Name" => sortOrder == "asc" ? query.OrderBy(w => w.Name) : query.OrderByDescending(w => w.Name),
        "Brand" => sortOrder == "asc" ? query.OrderBy(w => w.Brand) : query.OrderByDescending(w => w.Brand),
        "Category" => sortOrder == "asc" ? query.OrderBy(w => w.Category) : query.OrderByDescending(w => w.Category),
        "Country" => sortOrder == "asc" ? query.OrderBy(w => w.Country) : query.OrderByDescending(w => w.Country),
        "Year" => sortOrder == "asc" ? query.OrderBy(w => w.Year) : query.OrderByDescending(w => w.Year),
        "Price" => sortOrder == "asc" ? query.OrderBy(w => w.Price) : query.OrderByDescending(w => w.Price),
        _ => query.OrderBy(w => w.Name)
    };

    // Get total count for pagination
    var totalItems = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    // Ensure page is within valid range
    page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

    // Get paginated results
    var wines = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // Get distinct categories for filter dropdown
    var categories = await _context.Wines
        .Where(w => !string.IsNullOrEmpty(w.Category))
        .Select(w => w.Category)
        .Distinct()
        .OrderBy(c => c)
        .ToListAsync();

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

    return View(wines);
}


        // GET: Admin/CreateWine
        [Authorize(Roles = "Admin")]
        public IActionResult CreateWine()
        {
            return View();
        }

        // POST: Admin/CreateWine
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWine(Wine wine)
        {
            if (ModelState.IsValid)
            {
                _context.Wines.Add(wine);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wine added successfully!";
                return RedirectToAction(nameof(Wines));
            }
            return View(wine);
        }

        // GET: Admin/EditWine/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id)
        {
            var wine = await _context.Wines.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.Id == id);
            if (wine == null)
            {
                return NotFound();
            }
            return View(wine);
        }

        // POST: Admin/EditWine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWine(int id, Wine wine)
        {
            if (id != wine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wine);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Wine updated successfully!";
                    return RedirectToAction(nameof(Wines));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Wines.AnyAsync(w => w.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(wine);
        }

        // POST: Admin/DeleteWine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);
            if (wine != null)
            {
                wine.IsDeleted = true;
                wine.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
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
            var wine = await _context.Wines.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.Id == id);
            if (wine != null && wine.IsDeleted)
            {
                wine.IsDeleted = false;
                wine.DeletedAt = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wine restored successfully!";
            }
            return RedirectToAction(nameof(Wines), new { showDeleted = true });
        }

        // ==================== CONTACT FORM MANAGEMENT ====================

        // GET: Admin/Contacts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Contacts()
        {
            var contacts = await _context.ContactForms
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();
            return View(contacts);
        }

        // GET: Admin/ContactDetails/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ContactDetails(int id)
        {
            var contact = await _context.ContactForms.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            // Mark as read
            if (!contact.IsRead)
            {
                contact.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(contact);
        }

        // POST: Admin/ReplyContact/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyContact(int id, string replyMessage)
        {
            var contact = await _context.ContactForms.FindAsync(id);
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
                            {replyMessage.Replace("\n", "<br>")}
                        </div>
                        <p><strong>Your original message:</strong></p>
                        <p style='color: #666;'>{contact.Description}</p>
                        <hr style='border: none; border-top: 1px solid #e8e6e3; margin: 20px 0;'>
                        <p style='color: #999; font-size: 12px;'>
                            Best regards,<br>
                            Brox Distribution Team<br>
                            <a href='mailto:admin@broxdistribution.com'>admin@broxdistribution.com</a>
                        </p>
                    </body>
                    </html>
                ";

                await _emailService.SendEmailAsync(contact.Email, subject, emailBody);

                // Update contact form
                contact.IsReplied = true;
                contact.RepliedAt = DateTime.Now;
                contact.ReplyMessage = replyMessage;
                await _context.SaveChangesAsync();

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
            var contact = await _context.ContactForms.FindAsync(id);
            if (contact != null)
            {
                _context.ContactForms.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contact deleted successfully!";
            }
            return RedirectToAction(nameof(Contacts));
        }
    }
}
