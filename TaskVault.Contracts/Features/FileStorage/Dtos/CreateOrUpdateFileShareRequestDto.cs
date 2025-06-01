namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class CreateOrUpdateFileShareRequestDto
{
    public required IEnumerable<Guid> ToUsers { get; set; }
    public required Guid FileId { get; set; }
}