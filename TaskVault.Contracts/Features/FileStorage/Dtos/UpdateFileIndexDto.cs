namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class UpdateFileIndexDto
{
    public required Guid FileId { get; set; }
    public required int NewIndex { get; set; }
}