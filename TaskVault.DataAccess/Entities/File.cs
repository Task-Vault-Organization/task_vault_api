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
    
    public required bool IsDirectory { get; set; }

    [MaxLength(100000)]
    public string? HistoryJson { get; set; } = string.Empty;

    public virtual User? Uploader { get; set; }
    
    public virtual FileType? FileType { get; set; }

    public virtual IEnumerable<User>? Owners { get; set; }
    
    public virtual ICollection<DirectoryEntry>? AsDirectoryEntries { get; set; }
    
    public virtual ICollection<DirectoryEntry>? AsFileEntries { get; set; }

    public static File Create(Guid id, double size, string name, Guid uploaderId, DateTime uploadedAt, int fileTypeId, bool isDirectory = false)
    {
        return new File
        {
            Id = id,
            Size = size,
            Name = name,
            UploaderId = uploaderId,
            UploadedAt = uploadedAt,
            FileTypeId = fileTypeId,
            IsDirectory = isDirectory,
        };
    }
}