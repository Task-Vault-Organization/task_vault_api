using Newtonsoft.Json;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Shared.Helpers;

public static class FileHistoryHelper
{
    public static void AddFileHistoryLog(File file, FileHistoryLog fileHistoryLog)
    {
        var historyLog = new List<FileHistoryLog>();
        if (!string.IsNullOrEmpty(file.HistoryJson))
            historyLog = JsonConvert.DeserializeObject<List<FileHistoryLog>>(file.HistoryJson);
        historyLog?.Add(fileHistoryLog);
        file.HistoryJson = JsonConvert.SerializeObject(historyLog);
    }
}