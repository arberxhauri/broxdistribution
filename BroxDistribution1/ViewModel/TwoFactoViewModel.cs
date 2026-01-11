using System.ComponentModel.DataAnnotations;

namespace BroxDistribution1.ViewModel;

public class TwoFactorViewModel
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; }

    public string Email { get; set; }
        
    public bool RememberMe { get; set; }
}