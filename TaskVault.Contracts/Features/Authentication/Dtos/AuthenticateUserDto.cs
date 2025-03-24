namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class AuthenticateUserDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}