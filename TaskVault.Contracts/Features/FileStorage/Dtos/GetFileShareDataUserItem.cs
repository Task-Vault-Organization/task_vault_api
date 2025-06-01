using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileShareDataUserItem
{
    public required Guid UserId { get; set; }
    public required string UserEmail { get; set; }
    public FileShareRequestStatus? Status { get; set; }

    public static GetFileShareDataUserItem Create(Guid userId, string userEmail, FileShareRequestStatus? status)
    {
        return new GetFileShareDataUserItem()
        {
            UserId = userId,
            UserEmail = userEmail,
            Status = status
        };
    }
}