using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class User
{
    [Key]
    public required Guid Id { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }
    
    public virtual IEnumerable<File>? Files { get; set; }
}