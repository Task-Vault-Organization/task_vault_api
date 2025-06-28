namespace TaskVault.Business.Shared.Options;

public class FileUploadOptions
{
    public int MaxNoOfFilesToUploadOnce { get; set; }
    public int MaxFileToUploadSize { get; set; }
    public int MaxTotalFileSize { get; set; }
}