using System.ComponentModel.DataAnnotations;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class CreateUserDto
{
    [EmailAddress]
    public required string Email { get; set; }
    [MinLength(8)]
    [MaxLength(20)]
    public required string Password { get; set; }
}