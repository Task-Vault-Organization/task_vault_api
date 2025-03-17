using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class FileType
{
    [Key]
    public int Id { get; set; }

    public required string Extension { get; set; }

    public required string Name { get; set; }
}