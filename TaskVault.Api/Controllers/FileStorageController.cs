using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using Tesseract;
using IronOcr;
using TaskVault.Contracts.Features.FileStorage.Dtos;

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
    public async Task<IActionResult> UploadFileAsync([FromForm] UploadFileDto uploadFileDto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileStorageService.UploadFileAsync(userEmail, uploadFileDto));
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFileAsync(Guid fileId)
    {
        var downloadFileResponse = await _fileStorageService.DownloadFileAsync(fileId);
        return File(downloadFileResponse.FileMemoryStream.ToArray(), 
            downloadFileResponse.ContentType ?? "application/octet-stream", 
            fileId.ToString());
    }
    
    [Authorize]
    [HttpGet("uploaded")]
    public async Task<IActionResult> GetAllUploadedFilesAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllUploadedFilesAsync(userEmail));
    }
    
    [Authorize]
    [HttpGet("uploaded/directory")]
    public async Task<IActionResult> GetAllUploadedFilesAsync(string? directoryName)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllUploadedDirectoryFilesAsync(userEmail, directoryName));
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
    [HttpGet("file-types")]
    public async Task<IActionResult> GetFileTypesAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllFileTypesAsync(userEmail));
    }
    
    [Authorize]
    [HttpGet("file-categories")]
    public async Task<IActionResult> GetFileCategoriesAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.GetAllFileCategoriesAsync(userEmail));
    }
    
    [Authorize]
    [HttpDelete("{fileID}")]
    public async Task<IActionResult> DeleteFileAsync(Guid fileId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileService.DeleteUploadedFileAsync(userEmail, fileId));
    }
    
    [HttpPost("extract-text")]
    public async Task<IActionResult> ExtractText(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            var ocr = new IronTesseract();
        
            using var input = new OcrInput();

            using (var stream = image.OpenReadStream())
            {
                input.AddImage(stream);
            }

            OcrResult result = ocr.Read(input);
            string extractedText = result.Text;

            return Ok(new { text = extractedText });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing image: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("create-directory")]
    public async Task<IActionResult> CreateDirectoryAsync([FromBody] CreateDirectoryDto createDirectoryDto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _fileStorageService.CreateDirectoryAsync(userEmail, createDirectoryDto));
    }

    private static string PerformOcr(byte[] imageBytes)
    {
        using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        using var img = Pix.LoadFromMemory(imageBytes);
        using var page = engine.Process(img);
        return page.GetText();
    }
}