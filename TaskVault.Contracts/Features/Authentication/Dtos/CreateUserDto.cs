using System.ComponentModel.DataAnnotations;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class CreateUserDto
{
    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }
    
    [MaxLength(100)]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Full name can only contain letters, spaces, hyphens, and apostrophes.")]
    public required string FullName { get; set; }
    
    [MinLength(8)]
    [MaxLength(20)]
    public required string Password { get; set; }
}