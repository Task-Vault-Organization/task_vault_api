using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileDto
{
    public Guid Id { get; set; }

    public required double Size { get; set; }

    public required string Name { get; set; }

    public required Guid UploaderId { get; set; }

    public required DateTime UploadedAt { get; set; }

    public required int FileTypeId { get; set; }
    
    public FileType? FileType { get; set; }
    
    public GetUserDto? Uploader { get; set; }
    
    public IEnumerable<GetUserDto>? Owners { get; set; }
}