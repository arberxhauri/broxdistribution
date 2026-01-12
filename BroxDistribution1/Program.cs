using BroxDistribution;
using BroxDistribution.Models;
using BroxDistribution.Repositories;
using BroxDistribution1.Models;
using BroxDistribution1.Repositories;
using BroxDistribution1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                CreatedAt = DateTime.Now
            });
            Console.WriteLine("✅ Admin user created! Username: admin, Password: Admin@123");
        }
        else
        {
            Console.WriteLine("ℹ️ Admin already exists, skipping...");
        }

        // Seed Wines
        Console.WriteLine("🍷 Getting WineRepository...");
        var wineRepo = scopedServices.GetRequiredService<WineRepository>();
        
        Console.WriteLine("🍷 Calling GetAllAsync for wines...");
        var wines = await wineRepo.GetAllAsync();
        Console.WriteLine($"🍷 Found {wines.Count()} existing wines");
        
        if (!wines.Any())
        {
            Console.WriteLine("🍷 Adding wine 1...");
            await wineRepo.AddAsync(new Wine
            {
                Name = "Cabernet Sauvignon Reserve",
                Brand = "Château Example",
                Category = "Red Wine",
                Country = "France",
                Region = "Bordeaux",
                Grape = "Cabernet Sauvignon",
                Year = 2020,
                AlcoholPercentage = 14.5m,
                Price = 45.99m,
                Description = "Full-bodied red with rich tannins and notes of blackcurrant",
                ImageUrl = "/images/wines/cabernet.jpg",
                IsDeleted = false
            });
            Console.WriteLine("🍷 Wine 1 added");

            Console.WriteLine("🍷 Adding wine 2...");
            await wineRepo.AddAsync(new Wine
            {
                Name = "Chardonnay Grand Cru",
                Brand = "Domaine Pierre",
                Category = "White Wine",
                Country = "France",
                Region = "Burgundy",
                Grape = "Chardonnay",
                Year = 2021,
                AlcoholPercentage = 13.0m,
                Price = 38.50m,
                Description = "Elegant white wine with citrus and oak aromas",
                ImageUrl = "/images/wines/chardonnay.jpg",
                IsDeleted = false
            });
            Console.WriteLine("🍷 Wine 2 added");

            Console.WriteLine("🍷 Adding wine 3...");
            await wineRepo.AddAsync(new Wine
            {
                Name = "Prosecco DOC",
                Brand = "Villa Sandi",
                Category = "Sparkling Wine",
                Country = "Italy",
                Region = "Veneto",
                Grape = "Glera",
                Year = 2022,
                AlcoholPercentage = 11.0m,
                Price = 18.99m,
                Description = "Fresh and fruity sparkling wine with floral notes",
                ImageUrl = "/images/wines/prosecco.jpg",
                IsDeleted = false
            });
            Console.WriteLine("🍷 Wine 3 added");

            Console.WriteLine("✅ Sample wines created!");
        }
        else
        {
            Console.WriteLine($"ℹ️ {wines.Count()} wines already exist, skipping...");
        }

        Console.WriteLine("✅ Seed process completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seed error: {ex.Message}");
        Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");
        Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        throw;
    }
}
