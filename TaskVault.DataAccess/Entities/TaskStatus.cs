using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class TaskStatus
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }
}