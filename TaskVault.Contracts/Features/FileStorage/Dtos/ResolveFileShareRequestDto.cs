namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class ResolveFileShareRequestDto
{
    public required Guid FileShareRequestId { get; set; }
    public required int ResponseStatusId { get; set; }
}