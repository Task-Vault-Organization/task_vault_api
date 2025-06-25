using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Shared.Services;

public class UsersService : IUsersService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;
    private readonly IFileRepository _fileRepository;
    private readonly IFileTypeRepository _fileTypeRepository;

    public UsersService(IExceptionHandlingService exceptionHandlingService, IRepository<User> userRepository, IMapper mapper, IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions, IFileRepository fileRepository, IFileTypeRepository fileTypeRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _mapper = mapper;
        _s3Client = s3Client;
        _awsOptions = awsOptions.Value;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
    }

    public async Task<SearchUsersResponseDto> SearchUsersAsync(string userEmail, string searchField)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync((u) => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            if (searchField.Length < 3)
            {
                throw new ServiceException(StatusCodes.Status400BadRequest,
                    "Search field length must be at least 3 characters");
            }

            var usersFound = await _userRepository
                .FindAsync(u =>
                    u.Email.ToLower().Trim().Contains(searchField.ToLower().Trim())
                );

            var enumerable = usersFound as User[] ?? usersFound.ToArray();
            var result = enumerable.Select(u => _mapper.Map<GetUserDto>(u));


            var getUserDtos = result as GetUserDto[] ?? result.ToArray();
            var successMessage = getUserDtos.Any() ? "Successfully searched users" : "No user found";
            return SearchUsersResponseDto.Create(successMessage, getUserDtos);
        }, "Error when searching for users");
    }
    
    public async Task<BaseApiResponse> UploadProfilePhotoAsync(string userEmail, IFormFile profilePhoto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (user == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            if (profilePhoto.Length == 0)
                throw new ServiceException(StatusCodes.Status400BadRequest, "Profile photo cannot be empty");

            var fileId = Guid.NewGuid();

            var putRequest = new PutObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileId.ToString(),
                InputStream = profilePhoto.OpenReadStream(),
                ContentType = profilePhoto.ContentType
            };
            await _s3Client.PutObjectAsync(putRequest);

            var fileType = (await _fileTypeRepository.FindAsync(ft => ft.Name == profilePhoto.ContentType)).FirstOrDefault();
            if (fileType == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "Unsupported file type");

            var fileEntity = File.Create(
                fileId,
                size: profilePhoto.Length,
                name: profilePhoto.FileName,
                uploaderId: user.Id,
                uploadedAt: DateTime.UtcNow,
                fileTypeId: fileType.Id
            );
            fileEntity.Owners = new List<User> { user };

            await _fileRepository.AddAsync(fileEntity);

            user.ProfilePhotoId = fileId;
            user.GoogleProfilePhotoUrl = null;
            await _userRepository.UpdateAsync(user, user.Id);

            return BaseApiResponse.Create("Profile photo uploaded successfully");
        }, "Error uploading profile photo");
    }

}