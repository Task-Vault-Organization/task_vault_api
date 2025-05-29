using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileHistoryResponseDto : BaseApiResponse
{
    public required IEnumerable<FileHistoryLog> FileHistoryLogs { get; set; }

    public static GetFileHistoryResponseDto Create(string message, IEnumerable<FileHistoryLog> fileHistoryLogs)
    {
        return new GetFileHistoryResponseDto()
        {
            Message = message,
            FileHistoryLogs = fileHistoryLogs
        };
    }
}