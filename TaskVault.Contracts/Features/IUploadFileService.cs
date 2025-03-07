namespace MsaCookingApp.Contracts.Features;

public interface IUploadFileService
{
    Task<string> Upload(Stream stream, string fileName);
    Task<string> GetFileUrl(string fileName);
}