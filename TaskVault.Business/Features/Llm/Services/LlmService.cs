using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.Llm.Abstractions;
using TaskVault.Contracts.Features.Llm.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Repositories.Abstractions;
using UglyToad.PdfPig;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Options;

namespace TaskVault.Business.Features.Llm.Services;

public class LlmService : ILlmService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IFileRepository _fileRepository;
    private readonly IFileTypeRepository _fileTypeRepository;
    private readonly IFileCategoryRepository _fileCategoryRepository;
    private readonly IEntityValidator _entityValidator;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;

    public LlmService(
        IExceptionHandlingService exceptionHandlingService,
        IFileRepository fileRepository,
        IFileTypeRepository fileTypeRepository,
        IEntityValidator entityValidator,
        IFileCategoryRepository fileCategoryRepository,
        IAmazonS3 s3Client,
        IOptions<AwsOptions> awsOptions)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
        _entityValidator = entityValidator;
        _fileCategoryRepository = fileCategoryRepository;
        _s3Client = s3Client;
        _awsOptions = awsOptions.Value;
    }


    public async Task<CheckFileMatchesCategoryResponseDto> CheckIfFileMatchesCategoryAsync(string userEmail, CheckFileMatchesCategoryDto dto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(dto.FileId);
            var category = await _fileCategoryRepository.GetByIdAsync(dto.FileCategoryId);

            if (category == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File category not found");

            if (file.FileTypeId != 5)
                throw new ServiceException(StatusCodes.Status400BadRequest, "Only PDF files are supported");

            var fileBytes = await GetFileBytesAsync(dto.FileId);
            string extractedText;

            using (var pdf = PdfDocument.Open(new MemoryStream(fileBytes)))
            {
                extractedText = string.Join("\n", pdf.GetPages().Select(p => p.Text));
            }

            var prompt = $"""
                          You are an AI assistant that checks how well a document matches a specific category.

                          The category is: "{category.Name}"

                          Before giving a result, think carefully: only return a high score if the document is clearly a strong example of this category.

                          Use this scoring scale:
                          - 90–100%: Clearly and definitely fits this category
                          - 70–89%: Possibly fits, but not certain
                          - 40–69%: Contains some related content, but not a real match
                          - 0–39%: Not related to the category

                          Examples:
                          - A resume for a job → Category: "CV" → 95%
                          - A generic letter or invoice → Category: "CV" → 10%
                          - A list of books → Category: "Invoice" → 5%
                          - A purchase receipt → Category: "Invoice" → 92%

                          Your output must be **only** a number followed by the percent sign. Nothing else. Example: `47%`

                          Do not guess. If unsure, choose a low percentage.

                          Document:
                          {extractedText}
                          """;

            using var httpClient = new HttpClient();
            var payload = new
            {
                model = "mistral",
                prompt = prompt,
                stream = false
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(payload));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync("http://localhost:11434/api/generate", requestContent);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            var rawResponse = doc.RootElement.GetProperty("response").GetString();

            var match = Regex.Match(rawResponse ?? "", @"(\d{1,3})\s*%");

            if (!match.Success || !double.TryParse(match.Groups[1].Value, out var percent))
                throw new ServiceException(StatusCodes.Status500InternalServerError, "Could not determine match percentage");

            return CheckFileMatchesCategoryResponseDto.Create("Analysis complete", percent);
        }, "Error when checking file");
    }

    private async Task<byte[]> GetFileBytesAsync(Guid fileId)
    {
        var request = new GetObjectRequest
        {
            BucketName = _awsOptions.BucketName,
            Key = fileId.ToString()
        };

        using var response = await _s3Client.GetObjectAsync(request);
        using var responseStream = response.ResponseStream;
        await using var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}