namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class CreateOrUpdateFileShareRequestDto
{
    public required IEnumerable<string> ToUsersEmails { get; set; }
    public required Guid FileId { get; set; }
}