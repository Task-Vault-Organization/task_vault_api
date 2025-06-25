using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class User
{
    [Key]
    public required Guid Id { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }
    
    [MaxLength(100)] public required string FullName { get; set; }

    [MinLength(8)]
    [MaxLength(20)]
    public required string Password { get; set; }

    public Guid RootDirectoryId { get; set; }

    public string? GoogleId { get; set; }

    public required bool EmailConfirmed { get; set; } = false;

    public virtual IEnumerable<File>? Files { get; set; }

    public virtual IEnumerable<Task>? Tasks { get; set; }

    public virtual ICollection<DirectoryEntry>? DirectoryEntries { get; set; }

    public virtual ICollection<EmailConfirmationRequest>? EmailConfirmationRequests { get; set; }

    public static User Create(string email, string fullName, bool emailConfirmed, string password, string? googleId)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = password,
            FullName = fullName,
            GoogleId = googleId,
            EmailConfirmed = emailConfirmed
        };
    }
}