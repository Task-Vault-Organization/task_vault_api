namespace TaskVault.Contracts.Shared.Dtos;

public class GetUserDto
{
    public required Guid Id { get; set; }
    
    public required string Email { get; set; }
    public Guid? RootDirectoryId { get; set; }
    public Guid? ProfilePhotoId { get; set; }
    public string? GoogleProfilePhotoUrl { get; set; } = string.Empty;
    public required double TotalFileSize { get; set; }
}