using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.FileStorage.Abstractions;

namespace TaskVault.Api.Controllers;

[Route("api/file-storage")]
public class FileStorageController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileService _fileService;

    public FileStorageController(IFileStorageService fileStorageService, IFileService fileService)
    {
        _fileStorageService = fileStorageService;
        _fileService = fileService;
    }
    
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFileAsync(IFormFile file)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileStorageService.UploadFileAsync(userEmail, file));
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFileAsync(Guid fileId)
    {
        var downloadFileResponse = await _fileStorageService.DownloadFileAsync(fileId);
        return File(downloadFileResponse.FileMemoryStream.ToArray(), downloadFileResponse.ContentType ?? "", fileId.ToString());
    }
    
    [Authorize]
    [HttpGet("uploaded")]
    public async Task<IActionResult> GetAllUploadedFilesAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllUploadedFilesAsync(userEmail));
    }
    
    [Authorize]
    [HttpGet("shared")]
    public async Task<IActionResult> GetAllSharedFilesAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllSharedFilesAsync(userEmail));
    }
    
    [Authorize]
    [HttpGet("{fileID}")]
    public async Task<IActionResult> GetFileAsync(Guid fileId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetFileAsync(userEmail, fileId));
    }
    
    [Authorize]
    [HttpDelete("{fileID}")]
    public async Task<IActionResult> DeleteFileAsync(Guid fileId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.DeleteUploadedFileAsync(userEmail, fileId));
    }
}