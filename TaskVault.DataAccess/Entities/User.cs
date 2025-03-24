using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class User
{
    [Key]
    public required Guid Id { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }

    [MinLength(8)]
    [MaxLength(20)]
    public required string Password { get; set; }
    
    public virtual IEnumerable<File>? Files { get; set; }

    public static User Create(string email, string password)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = password
        };
    }
}