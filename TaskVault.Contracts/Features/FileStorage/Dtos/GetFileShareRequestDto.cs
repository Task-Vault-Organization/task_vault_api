using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileShareRequestDto
{
    public Guid Id { get; set; }

    public required Guid FromId { get; set; }
    public required Guid ToId { get; set; }
    public required Guid FileId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required int StatusId { get; set; }
    
    public GetUserDto? From { get; set; }
    public GetUserDto? To { get; set; }
    public GetFileDto? File { get; set; }
    public FileShareRequestStatus? Status { get; set; }
}