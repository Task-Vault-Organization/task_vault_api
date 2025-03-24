using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.FileStorage.Abstractions;

namespace TaskVault.Api.Controllers;

[Route("api/file-storage")]
[Authorize]
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
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileStorageService.UploadFileAsync(userEmail, file));
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFileAsync(Guid fileId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        var downloadFileResponse = await _fileStorageService.DownloadFileAsync(userEmail, fileId);
        return File(downloadFileResponse.FileMemoryStream.ToArray(), downloadFileResponse.ContentType ?? "", fileId.ToString());
    }
}