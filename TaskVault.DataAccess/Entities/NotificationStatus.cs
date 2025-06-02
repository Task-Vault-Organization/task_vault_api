using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class NotificationStatus
{
    [Key] public int Id { get; set; }
    public required string Status { get; set; }
}