namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskSubmissionUserDto
{
    public required Guid Id { get; set; }
    
    public required string Email { get; set; }
    public Guid? RootDirectoryId { get; set; }
    public bool? Approved { get; set; }
}