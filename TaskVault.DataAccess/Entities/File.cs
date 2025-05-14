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
    
    public required bool IsDirectory { get; set; } = false;

    public Guid? DirectoryId { get; set; }

    public virtual File? Directory { get; set; }

    public virtual ICollection<File>? Children { get; set; }

    public virtual User? Uploader { get; set; }
    
    public virtual FileType? FileType { get; set; }

    public virtual IEnumerable<User>? Owners { get; set; }

    public static File Create(Guid id, double size, string name, Guid uploaderId, DateTime uploadedAt, int fileTypeId, Guid? directoryId = null, bool isDirectory = false)
    {
        return new File
        {
            Id = id,
            Size = size,
            Name = name,
            UploaderId = uploaderId,
            UploadedAt = uploadedAt,
            FileTypeId = fileTypeId,
            DirectoryId = directoryId,
            IsDirectory = isDirectory
        };
    }
}