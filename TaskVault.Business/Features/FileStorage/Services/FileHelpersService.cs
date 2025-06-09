using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileHelpersService : IFileHelpersService
{
    private readonly IDirectoryEntryRepository _directoryEntryRepository;
    private readonly ILogger<FileHelpersService> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;

    public FileHelpersService(IDirectoryEntryRepository directoryEntryRepository, ILogger<FileHelpersService> logger, IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions)
    {
        _directoryEntryRepository = directoryEntryRepository;
        _logger = logger;
        _s3Client = s3Client;
        _awsOptions = awsOptions.Value;
    }

    public async Task CreateDirectoryEntryAsync(File file, Guid userId, Guid directoryId)
    {
        try
        {
            var newDirectoryEntry = DirectoryEntry.Create(userId, directoryId, file.Id, 0);
            var siblings = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == userId &&
                de.DirectoryId == directoryId);

            var directoryEntries = siblings as DirectoryEntry[] ?? siblings.ToArray();
            foreach (var directoryEntry in directoryEntries)
            {
                directoryEntry.Index++;
            }
            
            await Task.WhenAll(directoryEntries.Select(entry =>
                _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId })
            ));

            await _directoryEntryRepository.AddAsync(newDirectoryEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory entry");
            throw;
        }
    }
    
    public async Task DeleteDirectoryEntryAsync(File file, Guid userId, Guid directoryId)
    {
        try
        {
            var foundDirectoryEntry =
                await _directoryEntryRepository.GetByIdAsync(new object[] { userId, file.Id, directoryId });
            if (foundDirectoryEntry == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "Directory entry not found");
            }
            
            var siblings = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == userId &&
                de.DirectoryId == directoryId
                && de.Index > foundDirectoryEntry.Index);

            var directoryEntries = siblings as DirectoryEntry[] ?? siblings.ToArray();
            foreach (var directoryEntry in directoryEntries)
            {
                directoryEntry.Index--;
            }
            
            await Task.WhenAll(directoryEntries.Select(entry =>
                _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId })
            ));

            await _directoryEntryRepository.RemoveAsync(foundDirectoryEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory entry");
            throw;
        }
    }

    public string GenerateUniqueFileName(string originalName, List<DirectoryEntry> existingEntries)
    {
        try
        {
            var generatedNames = new HashSet<string>(existingEntries
                .Where(de => de.File != null)
                .Select(de => de.File!.Name));
            
            var baseName = Path.GetFileNameWithoutExtension(originalName);
            var extension = Path.GetExtension(originalName);
            var finalName = baseName + extension;
            int counter = 1;

            while (generatedNames.Contains(finalName))
                finalName = $"{baseName}({counter++}){extension}";

            generatedNames.Add(finalName);
            return finalName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory entry");
            throw;
        }
    }
    
    public async Task UploadToS3Async(IFormFile formFile, Guid fileId)
    {
        using var stream = formFile.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _awsOptions.BucketName,
            Key = fileId.ToString(),
            InputStream = stream,
            ContentType = formFile.ContentType
        };
        await _s3Client.PutObjectAsync(request);
    }
}