using System.ComponentModel.DataAnnotations;

namespace BroxDistribution.Models
{
    public class ContactForm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000)]
        public string Description { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        public bool IsReplied { get; set; } = false;
        public DateTime? RepliedAt { get; set; }
        public string? ReplyMessage { get; set; }

        // Wine inquiry fields (optional)
        [StringLength(200)]
        public string? WineName { get; set; }

        [StringLength(100)]
        public string? WineBrand { get; set; }

        [StringLength(50)]
        public string? WineCategory { get; set; }

        public int? WineYear { get; set; }

        [StringLength(100)]
        public string? WineCountry { get; set; }
    }
}