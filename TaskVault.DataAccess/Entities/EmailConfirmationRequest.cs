using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class EmailConfirmationRequest
{
    [Key]
    public Guid Id { get; set; }

    public required Guid UserId { get; set; }

    public required DateTime ExpiresAt { get; set; }

    public required bool Confirmed { get; set; } = false;

    [MaxLength(10)]
    public required string CodeToVerify { get; set; }

    public virtual User? User { get; set; }

    public static EmailConfirmationRequest Create(Guid userId, DateTime expiresAt, bool confirmed, string codeToVerify)
    {
        return new EmailConfirmationRequest()
        {
            UserId = userId,
            ExpiresAt = expiresAt,
            Confirmed = confirmed,
            CodeToVerify = codeToVerify
        };
    }
}