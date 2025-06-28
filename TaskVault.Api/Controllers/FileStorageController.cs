using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Api.Controllers
{
    [Route("api/file-storage")]
    public class FileStorageController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileService _fileService;
        private readonly IFileSharingService _fileSharingService;

        public FileStorageController(IFileStorageService fileStorageService, IFileService fileService, IFileSharingService fileSharingService)
        {
            _fileStorageService = fileStorageService;
            _fileService = fileService;
            _fileSharingService = fileSharingService;
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
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            var downloadFileResponse = await _fileStorageService.DownloadFileAsync(userEmail, fileId);
            return File(downloadFileResponse.FileMemoryStream.ToArray(),
                downloadFileResponse.ContentType ?? "application/octet-stream",
                fileId.ToString());
        }

        [Authorize]
        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFileAsync(Guid fileId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetFileAsync(userEmail, fileId));
        }

        [Authorize]
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFileAsync(Guid fileId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileStorageService.DeleteFileAsync(userEmail, fileId));
        }

        [Authorize]
        [HttpPatch("file/rename")]
        public async Task<IActionResult> RenameFileAsync([FromBody] RenameFileDto renameFileDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.RenameFileAsync(userEmail, renameFileDto));
        }

        [Authorize]
        [HttpGet("{fileId}/history")]
        public async Task<IActionResult> GetFileHistoryAsync(Guid fileId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetFileHistoryAsync(userEmail, fileId));
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
        [HttpPost("file-categories/custom")]
        public async Task<IActionResult> CreateCustomFileCategoryAsync([FromBody] CreateCustomFileCategoryDto dto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileStorageService.CreateCustomFileCategoryAsync(userEmail, dto));
        }

        [Authorize]
        [HttpDelete("file-categories/custom/{categoryId}")]
        public async Task<IActionResult> DeleteCustomFileCategoryAsync(int categoryId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileStorageService.DeleteCustomFileCategoryAsync(userEmail, categoryId));
        }

        [Authorize]
        [HttpPost("create-directory")]
        public async Task<IActionResult> CreateDirectoryAsync([FromBody] CreateDirectoryDto createDirectoryDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileStorageService.CreateDirectoryAsync(userEmail, createDirectoryDto));
        }

        [Authorize]
        [HttpPost("files/update-index")]
        public async Task<IActionResult> UpdateFileIndexAsync([FromBody] UpdateFileIndexDto updateFileIndexDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileStorageService.UpdateFileIndexAsync(userEmail, updateFileIndexDto));
        }

        [Authorize]
        [HttpGet("directory/{directoryId}/files/all")]
        public async Task<IActionResult> GetAllDirectoryFilesAsync(Guid directoryId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetAllDirectoryFilesAsync(userEmail, directoryId));
        }

        [Authorize]
        [HttpGet("directory/{directoryId}/files/uploaded")]
        public async Task<IActionResult> GetUploadedFilesInDirectoryAsync(Guid directoryId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetUploadedFilesInDirectoryAsync(userEmail, directoryId));
        }

        [Authorize]
        [HttpGet("directory/{directoryId}/files/shared")]
        public async Task<IActionResult> GetSharedFilesInDirectoryAsync(Guid directoryId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetSharedFilesInDirectoryAsync(userEmail, directoryId));
        }

        [Authorize]
        [HttpGet("files/all")]
        public async Task<IActionResult> GetAllUserFilesAsync()
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetAllUserFilesAsync(userEmail));
        }

        [Authorize]
        [HttpGet("files/uploaded")]
        public async Task<IActionResult> GetUploadedFilesByUserAsync()
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetUploadedFilesByUserAsync(userEmail));
        }

        [Authorize]
        [HttpGet("files/shared")]
        public async Task<IActionResult> GetSharedFilesAsync()
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetFilesSharedWithUserAsync(userEmail));
        }

        [Authorize]
        [HttpPost("file-share-requests")]
        public async Task<IActionResult> CreateFileShareRequestAsync([FromBody] CreateOrUpdateFileShareRequestDto createOrUpdateFileShareRequestDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileSharingService.CreateOrUpdateFileShareRequestAsync(userEmail, createOrUpdateFileShareRequestDto));
        }

        [Authorize]
        [HttpPatch("file-share-requests/resolve")]
        public async Task<IActionResult> ResolveFileShareRequestAsync([FromBody] ResolveFileShareRequestDto resolveFileShareRequestDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileSharingService.ResolveFileShareRequestAsync(userEmail, resolveFileShareRequestDto));
        }

        [Authorize]
        [HttpGet("file-share/{fileId}")]
        public async Task<IActionResult> GetFileShareDataAsync(Guid fileId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileSharingService.GetFileShareDataAsync(userEmail, fileId));
        }

        [Authorize]
        [HttpPost("file-share/file/move")]
        public async Task<IActionResult> MoveFileToDirectoryAsync(MoveFileToDirectoryDto moveFileToDirectoryDto)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.MoveFileToDirectoryAsync(userEmail, moveFileToDirectoryDto));
        }

        [Authorize]
        [HttpGet("files/by-type/{fileTypeId}")]
        public async Task<IActionResult> GetFilesByTypeAsync(int fileTypeId)
        {
            var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
            return Ok(await _fileService.GetFilesByTypeAsync(userEmail, fileTypeId));
        }
    }
}