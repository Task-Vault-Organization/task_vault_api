using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class FileHistoryLog
{
    public required DateTime LoggedAt { get; set; }
    public GetUserDto? ActionMadeBy { get; set; }
    public required string HistoryLog { get; set; }

    public static FileHistoryLog Create(string historyLog, GetUserDto? actionMadeBy)
    {
        return new FileHistoryLog()
        {
            LoggedAt = DateTime.Now,
            HistoryLog = historyLog,
            ActionMadeBy = actionMadeBy
        };
    }
}