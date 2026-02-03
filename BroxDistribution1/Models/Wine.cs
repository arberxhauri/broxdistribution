using System.ComponentModel.DataAnnotations;

namespace BroxDistribution.Models
{
    public class Wine
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string Name { get; set; }

        [Required, StringLength(128)]
        public string Brand { get; set; }

        [Required, StringLength(64)]
        public string Category { get; set; } // Red, White, Sparkling, etc.

        [StringLength(64)]
        public string Country { get; set; }

        [StringLength(64)]
        public string Region { get; set; }

        [StringLength(64)]
        public string Grape { get; set; }

        public int? Year { get; set; }

        [Range(0, 100)]
        public decimal AlcoholPercentage { get; set; }

        [StringLength(256)]
        public string ImageUrl { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
