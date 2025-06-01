using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class FileShareRequestStatus
{
    [Key] public int Id { get; set; }
    [MaxLength(10)] public required string Name { get; set; }
}