using Microsoft.AspNetCore.Http;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class UploadFileDto
{
    public required IFormFile File { get; set; }
    public Guid DirectoryId { get; set; }
}