using System.ComponentModel.DataAnnotations;

namespace BroxDistribution1.Models;

public class TwoFactorCode
{
    public int Id { get; set; }

    public int AdminId { get; set; }

    [Required]
    [StringLength(6)]
    public string Code { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    public DateTime ExpiresAt { get; set; }
        
    public bool IsUsed { get; set; } = false;
        
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public Admin Admin { get; set; }
}