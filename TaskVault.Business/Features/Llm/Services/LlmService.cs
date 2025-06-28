using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.Llm.Abstractions;
using TaskVault.Contracts.Features.Llm.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Repositories.Abstractions;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Options;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Firebase.Auth;
using TaskVault.Contracts.Features.FileStorage.Abstractions;

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
    private readonly Dictionary<int, IFileTextExtractor> _extractors;
    private readonly IDirectoryEntryRepository _directoryEntryRepository;
    private readonly IFileHelpersService _fileHelpersService;
    private readonly ILlmProvider _llmProvider;

    public LlmService(
        IExceptionHandlingService exceptionHandlingService,
        IFileRepository fileRepository,
        IFileTypeRepository fileTypeRepository,
        IEntityValidator entityValidator,
        IFileCategoryRepository fileCategoryRepository,
        IAmazonS3 s3Client,
        IOptions<AwsOptions> awsOptions,
        IDirectoryEntryRepository directoryEntryRepository,
        IFileHelpersService fileHelpersService,
        ILlmProvider llmProvider)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
        _entityValidator = entityValidator;
        _fileCategoryRepository = fileCategoryRepository;
        _s3Client = s3Client;
        _directoryEntryRepository = directoryEntryRepository;
        _fileHelpersService = fileHelpersService;
        _llmProvider = llmProvider;
        _awsOptions = awsOptions.Value;
        _extractors = new Dictionary<int, IFileTextExtractor>
        {
            { 2, new ImageFileExtractor() },
            { 3, new ImageFileExtractor() },
            { 5, new PdfFileExtractor() }
        };
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

            var fileType = await _fileTypeRepository.GetByIdAsync(file.FileTypeId);
            if (fileType == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");

            if (!_extractors.TryGetValue(file.FileTypeId, out var extractor))
                throw new ServiceException(StatusCodes.Status400BadRequest, "Unsupported file type for categorization");

            var fileBytes = await GetFileBytesAsync(dto.FileId);
            var extractedText = extractor.ExtractText(fileBytes);

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

            var rawResponse = await _llmProvider.GenerateTextAsync(prompt);
            var match = Regex.Match(rawResponse ?? "", @"(\d{1,3})\s*%");

            if (!match.Success || !double.TryParse(match.Groups[1].Value, out var percent))
                throw new ServiceException(StatusCodes.Status500InternalServerError, "Could not determine match percentage");

            return CheckFileMatchesCategoryResponseDto.Create("Analysis complete", percent);
        }, "Error when checking file");
    }

    public async Task<JsonDocument> ExtractStructuredJsonFromImageAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);

            var fileType = await _fileTypeRepository.GetByIdAsync(file.FileTypeId);
            if (fileType == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");

            if (!_extractors.TryGetValue(file.FileTypeId, out var extractor))
                throw new ServiceException(StatusCodes.Status400BadRequest, "Unsupported file type for text extraction");

            var fileBytes = await GetFileBytesAsync(fileId);
            var extractedText = extractor.ExtractText(fileBytes);

            var prompt = $"""
                          You are an AI that extracts structured data from scanned documents.

                          Return a JSON object that represents the data in the following image text:

                          {extractedText}

                          Only return valid JSON. Do not explain.
                          """;

            var raw = await _llmProvider.GenerateTextAsync(prompt);
            if (string.IsNullOrWhiteSpace(raw))
                throw new ServiceException(StatusCodes.Status500InternalServerError, "LLM returned empty response");

            try
            {
                var json = JsonDocument.Parse(raw);
                return json;
            }
            catch
            {
                var fallbackJson = JsonSerializer.Serialize(new { raw = raw.Trim() });
                return JsonDocument.Parse(fallbackJson);
            }

        }, "Error extracting structured data from image");
    }

    public async Task<CategorizeFolderResponseDto> CategorizeFolderAsync(string userEmail, CategorizeFolderRequestDto dto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var folder = await _entityValidator.GetFileOrThrowAsync(dto.FolderId);
            if (!folder.IsDirectory)
                throw new ServiceException(StatusCodes.Status400BadRequest, "Must be a folder");

            var entries = await _directoryEntryRepository.FindAsync(de => de.DirectoryId == folder.Id && de.UserId == user.Id);
            var files = entries.Select(e => e.File).Where(f => f != null && !f.IsDirectory).ToList();

            var categorized = new Dictionary<string, List<TaskVault.DataAccess.Entities.File>>();
            foreach (var category in dto.Categories)
                categorized[category] = new();
            categorized["Other"] = new();

            foreach (var file in files)
            {
                var fileType = await _fileTypeRepository.GetByIdAsync(file!.FileTypeId);
                if (fileType == null || !_extractors.TryGetValue(file.FileTypeId, out var extractor))
                    continue;

                var fileBytes = await GetFileBytesAsync(file.Id);
                var extracted = extractor.ExtractText(fileBytes);

                var numberedCategories = dto.Categories.Select((c, i) => $"{i + 1}. {c}").ToList();
                var prompt = $"""
                You are a strict AI classification system. You will receive a numbered list of valid document categories and a document content. Your job is to return ONLY the number corresponding to the best fitting category. If the document does not match any category, return exactly 0. Do not explain. Do not output anything else.

                Categories:
                0. Other
                {string.Join("\n", numberedCategories)}

                Document:
                {extracted}
                """;

                var raw = await _llmProvider.GenerateTextAsync(prompt);
                if (!int.TryParse(raw?.Trim(), out int index) || index < 0 || index > dto.Categories.Count)
                    continue;

                string selected = index == 0 ? "Other" : dto.Categories[index - 1];
                categorized[selected].Add(file);
            }

            var resultMap = new Dictionary<string, List<Guid>>();

            foreach (var kvp in categorized)
            {
                if (!kvp.Value.Any()) continue;

                var existingDirEntry = await _directoryEntryRepository.FindAsync(de =>
                    de.DirectoryId == folder.Id &&
                    de.UserId == user.Id &&
                    de.File.IsDirectory &&
                    de.File.Name == kvp.Key);

                var entry = existingDirEntry.FirstOrDefault();
                var dir = entry?.File;

                if (dir == null)
                {
                    var conflict = await _directoryEntryRepository.FindAsync(de =>
                        de.File != null &&
                        de.DirectoryId == folder.Id &&
                        de.UserId == user.Id &&
                        de.File.Name == kvp.Key);

                    if (conflict.Any(de => de.File != null && !de.File.IsDirectory))
                        throw new ServiceException(StatusCodes.Status409Conflict, $"A file named '{kvp.Key}' already exists in the folder.");

                    dir = TaskVault.DataAccess.Entities.File.Create(Guid.NewGuid(), 0, kvp.Key, user.Id, DateTime.UtcNow, 8, true);
                    dir.Owners = folder.Owners != null ? new List<TaskVault.DataAccess.Entities.User>(folder.Owners) : new List<TaskVault.DataAccess.Entities.User> { user };
                    await _fileRepository.AddAsync(dir);
                    foreach (var owner in dir.Owners)
                        await _fileHelpersService.CreateDirectoryEntryAsync(dir, owner.Id, folder.Id);
                }

                foreach (var file in kvp.Value)
                {
                    if (file.Owners == null) file.Owners = new List<TaskVault.DataAccess.Entities.User>();
                    if (dir.Owners != null)
                        foreach (var owner in dir.Owners)
                        {
                            if (!file.Owners.Any(o => o.Id == owner.Id))
                                file.Owners.ToList().Add(owner);
                        }

                    if (dir.Owners != null)
                        foreach (var owner in dir.Owners)
                        {
                            var alreadyExists = (await _directoryEntryRepository.FindAsync(de =>
                                de.UserId == owner.Id && de.FileId == file.Id && de.DirectoryId == dir.Id)).Any();
                            if (!alreadyExists)
                                await _fileHelpersService.CreateDirectoryEntryAsync(file, owner.Id, dir.Id);
                        }

                    var currentEntry = await _directoryEntryRepository.FindAsync(de =>
                        de.UserId == user.Id && de.FileId == file.Id && de.DirectoryId == folder.Id);

                    foreach (var e in currentEntry.ToList())
                        await _directoryEntryRepository.RemoveAsync(e);
                }

                resultMap[kvp.Key] = kvp.Value.Select(f => f.Id).ToList();
            }

            return CategorizeFolderResponseDto.Create("Categorization complete", resultMap);
        }, "Error during folder categorization");
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