using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class NotificationType
{
    [Key] public int Id { get; set; }
    public required string Type { get; set; }
}