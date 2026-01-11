using BroxDistribution.Models;
using BroxDistribution1.Models;
using Microsoft.EntityFrameworkCore;

namespace BroxDistribution
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Wine> Wines { get; set; }
        public DbSet<ContactForm> ContactForms { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<TwoFactorCode> TwoFactorCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add query filter for soft delete
            modelBuilder.Entity<Wine>().HasQueryFilter(w => !w.IsDeleted);
            
            modelBuilder.Entity<TwoFactorCode>()
                .HasOne(t => t.Admin)
                .WithMany()
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed admin user (username: admin, password: Admin@123)
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@broxdistribution.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    CreatedAt = DateTime.Now
                }
            );

            // Seed wines - ALL with actual wine bottle images
            modelBuilder.Entity<Wine>().HasData(
                // RED WINES - Bottle images
                new Wine 
                { 
                    Id = 1, 
                    Name = "Château Margaux", 
                    Brand = "Château Margaux", 
                    Category = "Red", 
                    Country = "France", 
                    Region = "Bordeaux", 
                    Grape = "Cabernet Sauvignon", 
                    Year = 2015, 
                    AlcoholPercentage = 13.5m, 
                    Price = 850.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80", 
                    Description = "Elegant and complex with notes of blackcurrant, cedar, and vanilla. A legendary First Growth Bordeaux with exceptional aging potential." 
                },
                
                new Wine 
                { 
                    Id = 2, 
                    Name = "Barolo Riserva", 
                    Brand = "Giacomo Conterno", 
                    Category = "Red", 
                    Country = "Italy", 
                    Region = "Piedmont", 
                    Grape = "Nebbiolo", 
                    Year = 2016, 
                    AlcoholPercentage = 14.5m, 
                    Price = 320.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=800&q=80", 
                    Description = "Powerful yet refined with intense cherry, rose, and tar. The king of Italian wines with remarkable structure and longevity." 
                },
                
                new Wine 
                { 
                    Id = 3, 
                    Name = "Opus One", 
                    Brand = "Opus One Winery", 
                    Category = "Red", 
                    Country = "USA", 
                    Region = "Napa Valley", 
                    Grape = "Cabernet Sauvignon", 
                    Year = 2018, 
                    AlcoholPercentage = 14.5m, 
                    Price = 420.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1547595628-c61a29f496f0?w=800&q=80", 
                    Description = "A Bordeaux-Napa collaboration showcasing dark fruit, espresso, and silky tannins. Modern luxury in a bottle." 
                },
                
                new Wine 
                { 
                    Id = 4, 
                    Name = "Penfolds Grange", 
                    Brand = "Penfolds", 
                    Category = "Red", 
                    Country = "Australia", 
                    Region = "South Australia", 
                    Grape = "Shiraz", 
                    Year = 2017, 
                    AlcoholPercentage = 14.5m, 
                    Price = 650.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1586370434639-0fe43b2d32d6?w=800&q=80", 
                    Description = "Australia's most iconic wine with concentrated blackberry, chocolate, and exotic spices. Bold and age-worthy." 
                },
                
                new Wine 
                { 
                    Id = 5, 
                    Name = "Vega Sicilia Único", 
                    Brand = "Bodegas Vega Sicilia", 
                    Category = "Red", 
                    Country = "Spain", 
                    Region = "Ribera del Duero", 
                    Grape = "Tempranillo", 
                    Year = 2010, 
                    AlcoholPercentage = 14.0m, 
                    Price = 480.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1504279577054-acfeccf8fc52?w=800&q=80", 
                    Description = "Spain's most prestigious wine with deep cherry, leather, and tobacco. Extended aging creates extraordinary complexity." 
                },
                
                new Wine 
                { 
                    Id = 6, 
                    Name = "Pinot Noir Reserve", 
                    Brand = "Domaine de la Romanée-Conti", 
                    Category = "Red", 
                    Country = "France", 
                    Region = "Burgundy", 
                    Grape = "Pinot Noir", 
                    Year = 2019, 
                    AlcoholPercentage = 13.0m, 
                    Price = 1200.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1584916201218-f4242ceb4809?w=800&q=80", 
                    Description = "The pinnacle of Pinot Noir with ethereal red fruit, earth, and spice. Exceptionally rare and sought-after." 
                },
                
                new Wine 
                { 
                    Id = 7, 
                    Name = "Rioja Gran Reserva", 
                    Brand = "López de Heredia", 
                    Category = "Red", 
                    Country = "Spain", 
                    Region = "Rioja", 
                    Grape = "Tempranillo", 
                    Year = 2011, 
                    AlcoholPercentage = 13.5m, 
                    Price = 85.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1506377247377-2a5b3b417ebb?w=800&q=80", 
                    Description = "Traditional Rioja with dried cherry, vanilla, and leather. Elegant aging in American oak barrels." 
                },
                
                // WHITE WINES - Bottle images
                new Wine 
                { 
                    Id = 8, 
                    Name = "Chablis Grand Cru", 
                    Brand = "Domaine William Fèvre", 
                    Category = "White", 
                    Country = "France", 
                    Region = "Burgundy", 
                    Grape = "Chardonnay", 
                    Year = 2020, 
                    AlcoholPercentage = 13.0m, 
                    Price = 120.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1553361371-9b22f78e8b1d?w=800&q=80", 
                    Description = "Pure expression of Chardonnay with mineral-driven citrus, green apple, and oyster shell. Razor-sharp precision." 
                },
                
                new Wine 
                { 
                    Id = 9, 
                    Name = "Puligny-Montrachet", 
                    Brand = "Domaine Leflaive", 
                    Category = "White", 
                    Country = "France", 
                    Region = "Burgundy", 
                    Grape = "Chardonnay", 
                    Year = 2019, 
                    AlcoholPercentage = 13.5m, 
                    Price = 280.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1596464716127-f2a82984de30?w=800&q=80", 
                    Description = "World-class white Burgundy with hazelnut, white peach, and wet stone. Incredible depth and elegance." 
                },
                
                new Wine 
                { 
                    Id = 10, 
                    Name = "Riesling Spätlese", 
                    Brand = "Weingut Egon Müller", 
                    Category = "White", 
                    Country = "Germany", 
                    Region = "Mosel", 
                    Grape = "Riesling", 
                    Year = 2021, 
                    AlcoholPercentage = 8.0m, 
                    Price = 95.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1608181715566-82b365885969?w=800&q=80", 
                    Description = "Off-dry Riesling with honeyed apricot, lime zest, and slate. Perfect balance of sweetness and acidity." 
                },
                
                new Wine 
                { 
                    Id = 11, 
                    Name = "Cloudy Bay Sauvignon Blanc", 
                    Brand = "Cloudy Bay", 
                    Category = "White", 
                    Country = "New Zealand", 
                    Region = "Marlborough", 
                    Grape = "Sauvignon Blanc", 
                    Year = 2022, 
                    AlcoholPercentage = 13.0m, 
                    Price = 42.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1599785209707-a456fc1337bb?w=800&q=80", 
                    Description = "Iconic New Zealand Sauvignon with vibrant passionfruit, grapefruit, and fresh herbs. Crisp and refreshing." 
                },
                
                new Wine 
                { 
                    Id = 12, 
                    Name = "Meursault", 
                    Brand = "Domaine des Comtes Lafon", 
                    Category = "White", 
                    Country = "France", 
                    Region = "Burgundy", 
                    Grape = "Chardonnay", 
                    Year = 2020, 
                    AlcoholPercentage = 13.5m, 
                    Price = 195.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1585553616435-2dc0a54e271d?w=800&q=80", 
                    Description = "Rich yet refined with buttered toast, lemon curd, and hazelnut. Classic white Burgundy luxury." 
                },
                
                // SPARKLING WINES - Bottle images
                new Wine 
                { 
                    Id = 13, 
                    Name = "Dom Pérignon", 
                    Brand = "Moët & Chandon", 
                    Category = "Sparkling", 
                    Country = "France", 
                    Region = "Champagne", 
                    Grape = "Chardonnay", 
                    Year = 2012, 
                    AlcoholPercentage = 12.5m, 
                    Price = 220.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1558346490-a72e53ae2d4f?w=800&q=80", 
                    Description = "Prestige cuvée Champagne with brioche, white flowers, and citrus. Elegant mousse and remarkable aging potential." 
                },
                
                new Wine 
                { 
                    Id = 14, 
                    Name = "Krug Grande Cuvée", 
                    Brand = "Krug", 
                    Category = "Sparkling", 
                    Country = "France", 
                    Region = "Champagne", 
                    Grape = "Pinot Noir", 
                    Year = 2015, 
                    AlcoholPercentage = 12.0m, 
                    Price = 280.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1624362169293-f5d00c0bb81a?w=800&q=80", 
                    Description = "Multi-vintage blend of extraordinary complexity with toasted nuts, honey, and dried fruits. The ultimate Champagne." 
                },
                
                new Wine 
                { 
                    Id = 15, 
                    Name = "Bollinger Special Cuvée", 
                    Brand = "Bollinger", 
                    Category = "Sparkling", 
                    Country = "France", 
                    Region = "Champagne", 
                    Grape = "Pinot Noir", 
                    Year = 2018, 
                    AlcoholPercentage = 12.0m, 
                    Price = 75.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1560148489-a48d1076e7e6?w=800&q=80", 
                    Description = "Powerful and vinous with apple, pear, and spice. A favorite of James Bond and connoisseurs alike." 
                },
                
                new Wine 
                { 
                    Id = 16, 
                    Name = "Franciacorta Brut", 
                    Brand = "Ca' del Bosco", 
                    Category = "Sparkling", 
                    Country = "Italy", 
                    Region = "Lombardy", 
                    Grape = "Chardonnay", 
                    Year = 2019, 
                    AlcoholPercentage = 12.5m, 
                    Price = 55.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1598254436156-edo8bcd561bf?w=800&q=80", 
                    Description = "Italy's answer to Champagne with refined bubbles, white peach, and almond. Elegant and sophisticated." 
                },
                
                // ROSÉ WINES - Bottle images
                new Wine 
                { 
                    Id = 17, 
                    Name = "Château d'Esclans Whispering Angel", 
                    Brand = "Château d'Esclans", 
                    Category = "Rosé", 
                    Country = "France", 
                    Region = "Provence", 
                    Grape = "Grenache", 
                    Year = 2022, 
                    AlcoholPercentage = 13.0m, 
                    Price = 28.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1598524722937-291d2efee71d?w=800&q=80", 
                    Description = "The most famous Provence rosé with strawberry, watermelon, and fresh herbs. Crisp and refreshing summer wine." 
                },
                
                new Wine 
                { 
                    Id = 18, 
                    Name = "Domaines Ott Château de Selle", 
                    Brand = "Domaines Ott", 
                    Category = "Rosé", 
                    Country = "France", 
                    Region = "Provence", 
                    Grape = "Grenache", 
                    Year = 2022, 
                    AlcoholPercentage = 13.5m, 
                    Price = 48.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1598032895397-b9c35df5e0f0?w=800&q=80", 
                    Description = "Premium Provence rosé in an iconic bottle. Delicate red fruit, citrus, and mineral notes." 
                },
                
                // DESSERT WINES - Bottle images
                new Wine 
                { 
                    Id = 19, 
                    Name = "Château d'Yquem", 
                    Brand = "Château d'Yquem", 
                    Category = "Dessert", 
                    Country = "France", 
                    Region = "Sauternes", 
                    Grape = "Sémillon", 
                    Year = 2014, 
                    AlcoholPercentage = 13.5m, 
                    Price = 420.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1560148489-a48d1076e7e6?w=800&q=80", 
                    Description = "The world's greatest sweet wine with honeyed apricot, caramel, and botrytis complexity. Liquid gold." 
                },
                
                new Wine 
                { 
                    Id = 20, 
                    Name = "Royal Tokaji Essencia", 
                    Brand = "Royal Tokaji", 
                    Category = "Dessert", 
                    Country = "Hungary", 
                    Region = "Tokaj", 
                    Grape = "Furmint", 
                    Year = 2013, 
                    AlcoholPercentage = 5.0m, 
                    Price = 680.00m, 
                    ImageUrl = "https://images.unsplash.com/photo-1606216794079-e28fcdbb0ca2?w=800&q=80", 
                    Description = "Legendary Hungarian nectar with intense sweetness, orange marmalade, and spice. Ultra-rare and precious." 
                }
            );
        }
    }
}