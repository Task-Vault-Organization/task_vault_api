namespace TaskVault.Contracts.Shared.Dtos;

public class GetUserDto
{
    public required Guid Id { get; set; }
    
    public required string Email { get; set; }
}