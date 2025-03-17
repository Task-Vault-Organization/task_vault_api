using Microsoft.AspNetCore.Mvc;
using TaskVault.Contracts.Features.FileStorage;

namespace TaskVault.Api.Controllers;

public class FileStorageController : Controller
{
    private readonly IFileStorageService _fileStorageService;

    public FileStorageController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFileAsync(IFormFile file)
    {
        return Ok(await _fileStorageService.UploadFileAsync(file));
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFileAsync(string fileName)
    {
        var downloadFileResponse = await _fileStorageService.DownloadFileAsync(fileName);
        return File(downloadFileResponse.FileMemoryStream.ToArray(), downloadFileResponse.ContentType ?? "", fileName);
    }
}