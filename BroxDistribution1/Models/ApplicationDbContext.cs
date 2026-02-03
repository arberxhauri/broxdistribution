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
        }
    }
}