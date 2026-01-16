// Controllers/HomeController.cs
using System.Net;
using System.Text;
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using BroxDistribution1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BroxDistribution.Controllers
{
    public class HomeController : Controller
    {
        private readonly GraphMailSettings _mail;
        private readonly ContactRepository _contactRepository;
        private readonly IEmailService _emailService;

        public HomeController(
            IOptions<GraphMailSettings> mailOptions,
            ContactRepository contactRepository,
            IEmailService emailService)
        {
            _mail = mailOptions.Value;
            _contactRepository = contactRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Premium Wine Distribution in Albania | Brox Distribution";
            ViewData["MetaDescription"] =
                "Discover premium wine distribution for HoReCa, retail and logistics in the UK and Albania with Brox Distribution.";
            ViewData["OgTitle"] = "Brox Distribution - Premium Wine Selection";
            ViewData["OgDescription"] =
                "Expert wine distribution for hotels, restaurants and retail. Discover exceptional wines from world-renowned estates.";
            ViewData["OgType"] = "website";
            return View();
        }

        [HttpGet]
        public IActionResult Contact(string? wine, string? brand, string? category, int? year, string? country)
        {
            var model = new ContactForm();

            // Pre-fill wine information if coming from a wine details page
            if (!string.IsNullOrEmpty(wine))
            {
                model.WineName = wine;
                model.WineBrand = brand;
                model.WineCategory = category;
                model.WineYear = year;
                model.WineCountry = country;

                // Pre-fill the description with wine details
                var descriptionBuilder = new StringBuilder();
                descriptionBuilder.AppendLine("I am interested in the following wine:\n");
                descriptionBuilder.AppendLine($"Wine: {wine}");
                if (!string.IsNullOrEmpty(brand)) descriptionBuilder.AppendLine($"Producer: {brand}");
                if (!string.IsNullOrEmpty(category)) descriptionBuilder.AppendLine($"Category: {category}");
                if (year.HasValue) descriptionBuilder.AppendLine($"Vintage: {year}");
                if (!string.IsNullOrEmpty(country)) descriptionBuilder.AppendLine($"Country: {country}");
                descriptionBuilder.AppendLine("\nPlease provide pricing and availability information.\n\nAdditional details:\n");

                model.Description = descriptionBuilder.ToString();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactForm model)
        {
            ModelState.Remove("ReplyMessage");

            if (!ModelState.IsValid)
                return View(model);

            // Validate Graph mail settings
            if (string.IsNullOrWhiteSpace(_mail.SenderUpn) || string.IsNullOrWhiteSpace(_mail.AdminToEmail))
            {
                ModelState.AddModelError("", "Email configuration error. Please contact the administrator.");
                return View(model);
            }

            try
            {
                // Save to JSON file
                model.SubmittedAt = DateTime.UtcNow;
                await _contactRepository.AddAsync(model);

                // Send email to admin + confirmation to user (Graph, not SMTP)
                await SendAdminEmailAsync(model);
                await SendConfirmationEmailAsync(model);

                TempData["SuccessMessage"] = "Thank you for contacting Brox Distribution. We will get back to you shortly.";
                return RedirectToAction("ContactSuccess");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                ModelState.AddModelError("", "There was a problem sending your message. Please try again later.");
                return View(model);
            }
        }

        public IActionResult ContactSuccess()
        {
            return View();
        }

        private async Task SendAdminEmailAsync(ContactForm model)
        {
            var subject = model.WineName != null
                ? $"Wine Inquiry: {model.WineName}"
                : "New Contact Form Submission";

            var html = BuildAdminEmailHtml(model);

            await _emailService.SendEmailAsync(_mail.AdminToEmail, subject, html);
        }

        private async Task SendConfirmationEmailAsync(ContactForm model)
        {
            var subject = "Thank you for contacting Brox Distribution";

            var html = BuildConfirmationEmailHtml(model);

            await _emailService.SendEmailAsync(model.Email, subject, html);
        }

        private static string BuildAdminEmailHtml(ContactForm model)
        {
            // Minimal hardening: HTML-encode user input so the email can’t be injected with raw HTML.
            string Enc(string? s) => WebUtility.HtmlEncode(s ?? "");

            var subjectHeader = "New Contact Form Submission";

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<!DOCTYPE html>");
            bodyBuilder.AppendLine("<html><head><style>");
            bodyBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            bodyBuilder.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            bodyBuilder.AppendLine(".header { background: #1c1c1e; color: #FF5722; padding: 20px; text-align: center; }");
            bodyBuilder.AppendLine(".content { background: #f8f9fa; padding: 30px; margin-top: 20px; }");
            bodyBuilder.AppendLine(".field { margin-bottom: 15px; }");
            bodyBuilder.AppendLine(".field-label { font-weight: bold; color: #1c1c1e; }");
            bodyBuilder.AppendLine(".wine-info { background: #fff; border-left: 4px solid #FF5722; padding: 15px; margin: 20px 0; }");
            bodyBuilder.AppendLine(".message-box { background: #fff; padding: 20px; margin-top: 20px; border: 1px solid #ddd; }");
            bodyBuilder.AppendLine("</style></head><body>");
            bodyBuilder.AppendLine("<div class='container'>");
            bodyBuilder.AppendLine($"<div class='header'><h2>{subjectHeader}</h2></div>");
            bodyBuilder.AppendLine("<div class='content'>");

            // Contact Information
            bodyBuilder.AppendLine("<h3 style='color: #1c1c1e;'>Contact Information</h3>");
            bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Name:</span> {Enc(model.Name)}</div>");
            bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Email:</span> <a href='mailto:{Enc(model.Email)}'>{Enc(model.Email)}</a></div>");

            if (!string.IsNullOrEmpty(model.PhoneNumber))
                bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Phone:</span> {Enc(model.PhoneNumber)}</div>");

            if (!string.IsNullOrEmpty(model.Company))
                bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Company:</span> {Enc(model.Company)}</div>");

            // Wine Information (if applicable)
            if (!string.IsNullOrEmpty(model.WineName))
            {
                bodyBuilder.AppendLine("<div class='wine-info'>");
                bodyBuilder.AppendLine("<h3 style='color: #FF5722; margin-top: 0;'>Wine Inquiry</h3>");
                bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Wine:</span> {Enc(model.WineName)}</div>");

                if (!string.IsNullOrEmpty(model.WineBrand))
                    bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Producer:</span> {Enc(model.WineBrand)}</div>");

                if (!string.IsNullOrEmpty(model.WineCategory))
                    bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Category:</span> {Enc(model.WineCategory)}</div>");

                if (model.WineYear.HasValue)
                    bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Vintage:</span> {model.WineYear}</div>");

                if (!string.IsNullOrEmpty(model.WineCountry))
                    bodyBuilder.AppendLine($"<div class='field'><span class='field-label'>Country:</span> {Enc(model.WineCountry)}</div>");

                bodyBuilder.AppendLine("</div>");
            }

            // Message
            bodyBuilder.AppendLine("<div class='message-box'>");
            bodyBuilder.AppendLine("<h3 style='color: #1c1c1e; margin-top: 0;'>Message</h3>");

            // keep newlines but encode
            var safeDescription = Enc(model.Description).Replace("\n", "<br/>");
            bodyBuilder.AppendLine($"<p>{safeDescription}</p>");
            bodyBuilder.AppendLine("</div>");

            // Timestamp
            bodyBuilder.AppendLine($"<p style='margin-top: 20px; color: #666; font-size: 12px;'>Submitted at: {model.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");

            bodyBuilder.AppendLine("</div></div></body></html>");
            return bodyBuilder.ToString();
        }

        private static string BuildConfirmationEmailHtml(ContactForm model)
        {
            string Enc(string? s) => WebUtility.HtmlEncode(s ?? "");

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<!DOCTYPE html>");
            bodyBuilder.AppendLine("<html><head><style>");
            bodyBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            bodyBuilder.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            bodyBuilder.AppendLine(".header { background: #1c1c1e; color: #FF5722; padding: 30px; text-align: center; }");
            bodyBuilder.AppendLine(".content { background: #faf8f5; padding: 40px; margin-top: 20px; }");
            bodyBuilder.AppendLine(".wine-summary { background: #fff; border-left: 4px solid #FF5722; padding: 20px; margin: 20px 0; }");
            bodyBuilder.AppendLine(".footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }");
            bodyBuilder.AppendLine("</style></head><body>");
            bodyBuilder.AppendLine("<div class='container'>");

            // Header
            bodyBuilder.AppendLine("<div class='header'>");
            bodyBuilder.AppendLine("<h1 style='margin: 0; font-size: 28px;'>BROX DISTRIBUTION</h1>");
            bodyBuilder.AppendLine("<p style='margin: 10px 0 0 0; font-size: 14px; letter-spacing: 2px;'>PREMIUM WINE DISTRIBUTION</p>");
            bodyBuilder.AppendLine("</div>");

            // Content
            bodyBuilder.AppendLine("<div class='content'>");
            bodyBuilder.AppendLine($"<h2 style='color: #1c1c1e;'>Thank you, {Enc(model.Name)}!</h2>");
            bodyBuilder.AppendLine("<p>We have received your inquiry and our team will review it shortly.</p>");

            // Wine summary if applicable
            if (!string.IsNullOrEmpty(model.WineName))
            {
                bodyBuilder.AppendLine("<div class='wine-summary'>");
                bodyBuilder.AppendLine("<h3 style='color: #FF5722; margin-top: 0;'>Your Wine Inquiry</h3>");
                bodyBuilder.AppendLine($"<p style='font-size: 18px; margin: 10px 0;'><strong>{Enc(model.WineName)}</strong></p>");

                if (!string.IsNullOrEmpty(model.WineBrand))
                    bodyBuilder.AppendLine($"<p style='margin: 5px 0;'>Producer: {Enc(model.WineBrand)}</p>");

                if (!string.IsNullOrEmpty(model.WineCategory))
                    bodyBuilder.AppendLine($"<p style='margin: 5px 0;'>Category: {Enc(model.WineCategory)}</p>");

                if (model.WineYear.HasValue)
                    bodyBuilder.AppendLine($"<p style='margin: 5px 0;'>Vintage: {model.WineYear}</p>");

                bodyBuilder.AppendLine("</div>");
            }

            bodyBuilder.AppendLine("<p>One of our wine specialists will contact you within 24 hours with pricing and availability information.</p>");
            bodyBuilder.AppendLine("<p style='margin-top: 30px;'>Best regards,<br/><strong>The Brox Distribution Team</strong></p>");
            bodyBuilder.AppendLine("</div>");

            // Footer
            bodyBuilder.AppendLine("<div class='footer'>");
            bodyBuilder.AppendLine("<p>Brox Distribution | London, UK</p>");
            bodyBuilder.AppendLine("<p>Email: info@broxdistribution.com | Phone: +44 7899 313864</p>");
            bodyBuilder.AppendLine("</div>");

            bodyBuilder.AppendLine("</div></body></html>");
            return bodyBuilder.ToString();
        }
    }
}
