using BroxDistribution;
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using BroxDistribution1.Models;
using BroxDistribution1.Repositories;
using BroxDistribution1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// Add Email Service
builder.Services.AddHttpClient<IEmailService, EmailService>();

// Register JSON File Repositories as Singletons
builder.Services.AddSingleton<AdminRepository>();
builder.Services.AddSingleton<WineRepository>();
builder.Services.AddSingleton<ContactRepository>();

// Configure SMTP Settings
builder.Services.Configure<GraphMailSettings>(builder.Configuration.GetSection("GraphMail"));

var app = builder.Build();

// ⭐ FIX: Create a scope to resolve singleton services
await SeedInitialData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⭐ FIX: Add UseAuthentication BEFORE UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// SEO: Generate sitemap.xml dynamically
app.MapGet("/sitemap.xml", async (HttpContext context) =>
{
    var sb = new StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    // Static pages
    var staticPages = new[]
    {
        ("/",          1.0, "weekly"),
        ("/Products",  0.9, "daily"),
        ("/Home/Contact", 0.8, "monthly")
    };

    foreach (var (url, priority, changefreq) in staticPages)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>https://broxdistribution.co.uk{url}</loc>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine("  </url>");
    }

    sb.AppendLine("</urlset>");

    context.Response.ContentType = "application/xml";
    await context.Response.WriteAsync(sb.ToString());
});


app.Run();

async Task SeedInitialData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var scopedServices = scope.ServiceProvider;

    try
    {
        Console.WriteLine("🔄 Starting seed process...");
        
        // Get IWebHostEnvironment to check paths
        var env = scopedServices.GetRequiredService<IWebHostEnvironment>();
        var dataPath = Path.Combine(env.ContentRootPath, "App_Data");
        Console.WriteLine($"📂 Data folder path: {dataPath}");
        Console.WriteLine($"📂 Content root: {env.ContentRootPath}");

        // Ensure directory exists
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
            Console.WriteLine("✅ App_Data folder created");
        }
        else
        {
            Console.WriteLine("ℹ️ App_Data folder already exists");
        }

        // Seed Admin
        Console.WriteLine("👤 Getting AdminRepository...");
        var adminRepo = scopedServices.GetRequiredService<AdminRepository>();
        
        Console.WriteLine("👤 Calling GetAllAsync for admins...");
        var admins = await adminRepo.GetAllAsync();
        Console.WriteLine($"👤 Found {admins.Count()} existing admins");
        
        if (!admins.Any())
        {
            Console.WriteLine("👤 Adding admin user...");
            await adminRepo.AddAsync(new Admin
            {
                Username = "admin",
                Email = "admin@broxdistribution.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("sZI6pk</t:742qDw"),
                CreatedAt = DateTime.Now
            });
            Console.WriteLine("✅ Admin user created! Username: admin, Password: Admin@123");
        }
        else
        {
            Console.WriteLine("ℹ️ Admin already exists, skipping...");
        }
        
        var wineRepo = scopedServices.GetRequiredService<WineRepository>();
        
        Console.WriteLine("🍷 Calling GetAllAsync for wines...");
        var wines = await wineRepo.GetAllAsync();
        Console.WriteLine($"🍷 Found {wines.Count()} existing wines");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seed error: {ex.Message}");
        Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");
        Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        throw;
    }
}
