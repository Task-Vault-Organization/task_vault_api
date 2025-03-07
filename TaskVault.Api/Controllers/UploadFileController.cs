using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MsaCookingApp.Contracts.Features;

namespace MsaCookingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadFileService _uploadFileService;

        public UploadFileController(IUploadFileService uploadFileService)
        {
            _uploadFileService = uploadFileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;
                
                string fileUrl = await _uploadFileService.Upload(stream, file.FileName);
                return Ok(new { Url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error uploading file", Error = ex.Message });
            }
        }
        
        [HttpGet("retrieve/{fileName}")]
        public async Task<IActionResult> GetFileUrl(string fileName)
        {
            try
            {
                var fileUrl = await _uploadFileService.GetFileUrl(fileName);
                return Ok(new { Url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving file", Error = ex.Message });
            }
        }
    }
}