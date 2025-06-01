using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class FileShareRequest
{
    [Key]
    public Guid Id { get; set; }

    public required Guid FromId { get; set; }
    public required Guid ToId { get; set; }
    public required Guid FileId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required int StatusId { get; set; }

    public virtual User? From { get; set; }
    public virtual User? To { get; set; }
    public virtual File? File { get; set; }
    public virtual FileShareRequestStatus? Status { get; set; }
    
    public static FileShareRequest Create(Guid fromId, Guid toId, Guid fileId, int statusId)
    {
        return new FileShareRequest
        {
            Id = Guid.NewGuid(),
            FromId = fromId,
            ToId = toId,
            FileId = fileId,
            CreatedAt = DateTime.UtcNow,
            StatusId = statusId
        };
    }
}