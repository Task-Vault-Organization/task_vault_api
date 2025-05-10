using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class CustomFileCategory
{
    [Key] public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public Guid? ModelId { get; set; }

    public virtual User? User { get; set; }
}