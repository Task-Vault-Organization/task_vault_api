using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class FileCategory
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }
    public bool? Legacy { get; set; }
}