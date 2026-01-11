using System.ComponentModel.DataAnnotations;

namespace BroxDistribution1.Models;

public class Admin
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; }

    [Required, StringLength(256)]
    public string PasswordHash { get; set; }

    [Required, StringLength(128)]
    public string Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
}