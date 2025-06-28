using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class CustomFileCategory
{
    [Key] public int Id { get; set; }
    public required Guid UserId { get; set; }
    [MaxLength(20)]
    public required string Name { get; set; }
    public virtual User? User { get; set; }

    public static CustomFileCategory Create(int id, Guid userId, string name)
    {
        return new CustomFileCategory()
        {
            Id = id,
            UserId = userId,
            Name = name
        };
    }
}