using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroxDistribution1.Migrations
{
    /// <inheritdoc />
    public partial class FinalSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert all 20 wines
            migrationBuilder.InsertData(
                table: "Wines",
                columns: new[] { "Id", "Name", "Brand", "Category", "Country", "Region", "Grape", "Year", "AlcoholPercentage", "Price", "ImageUrl", "Description" },
                values: new object[,]
                {
                    // RED WINES
                    { 1, "Château Margaux", "Château Margaux", "Red", "France", "Bordeaux", "Cabernet Sauvignon", 2015, 13.5m, 850.00m, "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80", "Elegant and complex with notes of blackcurrant, cedar, and vanilla. A legendary First Growth Bordeaux with exceptional aging potential." },
                    
                    { 2, "Barolo Riserva", "Giacomo Conterno", "Red", "Italy", "Piedmont", "Nebbiolo", 2016, 14.5m, 320.00m, "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=800&q=80", "Powerful yet refined with intense cherry, rose, and tar. The king of Italian wines with remarkable structure and longevity." },
                    
                    { 3, "Opus One", "Opus One Winery", "Red", "USA", "Napa Valley", "Cabernet Sauvignon", 2018, 14.5m, 420.00m, "https://images.unsplash.com/photo-1547595628-c61a29f496f0?w=800&q=80", "A Bordeaux-Napa collaboration showcasing dark fruit, espresso, and silky tannins. Modern luxury in a bottle." },
                    
                    { 4, "Penfolds Grange", "Penfolds", "Red", "Australia", "South Australia", "Shiraz", 2017, 14.5m, 650.00m, "https://images.unsplash.com/photo-1586370434639-0fe43b2d32d6?w=800&q=80", "Australia's most iconic wine with concentrated blackberry, chocolate, and exotic spices. Bold and age-worthy." },
                    
                    { 5, "Vega Sicilia Único", "Bodegas Vega Sicilia", "Red", "Spain", "Ribera del Duero", "Tempranillo", 2010, 14.0m, 480.00m, "https://images.unsplash.com/photo-1504279577054-acfeccf8fc52?w=800&q=80", "Spain's most prestigious wine with deep cherry, leather, and tobacco. Extended aging creates extraordinary complexity." },
                    
                    { 6, "Pinot Noir Reserve", "Domaine de la Romanée-Conti", "Red", "France", "Burgundy", "Pinot Noir", 2019, 13.0m, 1200.00m, "https://images.unsplash.com/photo-1584916201218-f4242ceb4809?w=800&q=80", "The pinnacle of Pinot Noir with ethereal red fruit, earth, and spice. Exceptionally rare and sought-after." },
                    
                    { 7, "Rioja Gran Reserva", "López de Heredia", "Red", "Spain", "Rioja", "Tempranillo", 2011, 13.5m, 85.00m, "https://images.unsplash.com/photo-1506377247377-2a5b3b417ebb?w=800&q=80", "Traditional Rioja with dried cherry, vanilla, and leather. Elegant aging in American oak barrels." },
                    
                    // WHITE WINES
                    { 8, "Chablis Grand Cru", "Domaine William Fèvre", "White", "France", "Burgundy", "Chardonnay", 2020, 13.0m, 120.00m, "https://images.unsplash.com/photo-1553361371-9b22f78e8b1d?w=800&q=80", "Pure expression of Chardonnay with mineral-driven citrus, green apple, and oyster shell. Razor-sharp precision." },
                    
                    { 9, "Puligny-Montrachet", "Domaine Leflaive", "White", "France", "Burgundy", "Chardonnay", 2019, 13.5m, 280.00m, "https://images.unsplash.com/photo-1596464716127-f2a82984de30?w=800&q=80", "World-class white Burgundy with hazelnut, white peach, and wet stone. Incredible depth and elegance." },
                    
                    { 10, "Riesling Spätlese", "Weingut Egon Müller", "White", "Germany", "Mosel", "Riesling", 2021, 8.0m, 95.00m, "https://images.unsplash.com/photo-1608181715566-82b365885969?w=800&q=80", "Off-dry Riesling with honeyed apricot, lime zest, and slate. Perfect balance of sweetness and acidity." },
                    
                    { 11, "Cloudy Bay Sauvignon Blanc", "Cloudy Bay", "White", "New Zealand", "Marlborough", "Sauvignon Blanc", 2022, 13.0m, 42.00m, "https://images.unsplash.com/photo-1599785209707-a456fc1337bb?w=800&q=80", "Iconic New Zealand Sauvignon with vibrant passionfruit, grapefruit, and fresh herbs. Crisp and refreshing." },
                    
                    { 12, "Meursault", "Domaine des Comtes Lafon", "White", "France", "Burgundy", "Chardonnay", 2020, 13.5m, 195.00m, "https://images.unsplash.com/photo-1585553616435-2dc0a54e271d?w=800&q=80", "Rich yet refined with buttered toast, lemon curd, and hazelnut. Classic white Burgundy luxury." },
                    
                    // SPARKLING WINES
                    { 13, "Dom Pérignon", "Moët & Chandon", "Sparkling", "France", "Champagne", "Chardonnay", 2012, 12.5m, 220.00m, "https://images.unsplash.com/photo-1558346490-a72e53ae2d4f?w=800&q=80", "Prestige cuvée Champagne with brioche, white flowers, and citrus. Elegant mousse and remarkable aging potential." },
                    
                    { 14, "Krug Grande Cuvée", "Krug", "Sparkling", "France", "Champagne", "Pinot Noir", 2015, 12.0m, 280.00m, "https://images.unsplash.com/photo-1624362169293-f5d00c0bb81a?w=800&q=80", "Multi-vintage blend of extraordinary complexity with toasted nuts, honey, and dried fruits. The ultimate Champagne." },
                    
                    { 15, "Bollinger Special Cuvée", "Bollinger", "Sparkling", "France", "Champagne", "Pinot Noir", 2018, 12.0m, 75.00m, "https://images.unsplash.com/photo-1560148489-a48d1076e7e6?w=800&q=80", "Powerful and vinous with apple, pear, and spice. A favorite of James Bond and connoisseurs alike." },
                    
                    { 16, "Franciacorta Brut", "Ca' del Bosco", "Sparkling", "Italy", "Lombardy", "Chardonnay", 2019, 12.5m, 55.00m, "https://images.unsplash.com/photo-1598254436156-edo8bcd561bf?w=800&q=80", "Italy's answer to Champagne with refined bubbles, white peach, and almond. Elegant and sophisticated." },
                    
                    // ROSÉ WINES
                    { 17, "Château d'Esclans Whispering Angel", "Château d'Esclans", "Rosé", "France", "Provence", "Grenache", 2022, 13.0m, 28.00m, "https://images.unsplash.com/photo-1598524722937-291d2efee71d?w=800&q=80", "The most famous Provence rosé with strawberry, watermelon, and fresh herbs. Crisp and refreshing summer wine." },
                    
                    { 18, "Domaines Ott Château de Selle", "Domaines Ott", "Rosé", "France", "Provence", "Grenache", 2022, 13.5m, 48.00m, "https://images.unsplash.com/photo-1598032895397-b9c35df5e0f0?w=800&q=80", "Premium Provence rosé in an iconic bottle. Delicate red fruit, citrus, and mineral notes." },
                    
                    // DESSERT WINES
                    { 19, "Château d'Yquem", "Château d'Yquem", "Dessert", "France", "Sauternes", "Sémillon", 2014, 13.5m, 420.00m, "https://images.unsplash.com/photo-1560148489-a48d1076e7e6?w=800&q=80", "The world's greatest sweet wine with honeyed apricot, caramel, and botrytis complexity. Liquid gold." },
                    
                    { 20, "Royal Tokaji Essencia", "Royal Tokaji", "Dessert", "Hungary", "Tokaj", "Furmint", 2013, 5.0m, 680.00m, "https://images.unsplash.com/photo-1606216794079-e28fcdbb0ca2?w=800&q=80", "Legendary Hungarian nectar with intense sweetness, orange marmalade, and spice. Ultra-rare and precious." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all seeded wines
            migrationBuilder.DeleteData(
                table: "Wines",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });
        }
    }
}
