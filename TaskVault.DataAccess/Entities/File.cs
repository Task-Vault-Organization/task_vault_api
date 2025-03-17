using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class File
{
    [Key]
    public Guid Id { get; set; }

    public required double Size { get; set; }

    [MaxLength(20)]
    public required string Name { get; set; }

    public required Guid UploaderId { get; set; }

    public required DateTime UploadedAt { get; set; }

    public required int FileTypeId { get; set; }

    public virtual User? Uploader { get; set; }
    
    public virtual FileType? FileType { get; set; }

    public virtual IEnumerable<User>? Owners { get; set; }
}