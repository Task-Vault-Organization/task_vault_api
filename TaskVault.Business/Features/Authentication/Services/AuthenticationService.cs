using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.Authentication.Abstractions;
using TaskVault.Contracts.Features.Authentication.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Features.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly IMapper _mapper;
    private readonly IEntityValidator _entityValidator;
    private readonly IFileRepository _fileRepository;

    public AuthenticationService(
        IExceptionHandlingService exceptionHandlingService,
        IRepository<User> userRepository,
        IOptions<JwtOptions> jwtOptions,
        IMapper mapper,
        IEntityValidator entityValidator, IFileRepository fileRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
        _entityValidator = entityValidator;
        _fileRepository = fileRepository;
    }

    public async Task<BaseApiResponse> CreateUserAsync(CreateUserDto createUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            await _entityValidator.EnsureUserDoesNotExistAsync(createUserDto.Email);
            var user = User.Create(createUserDto.Email, createUserDto.Password);

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            await _userRepository.AddAsync(user);

            var rootFolderId = Guid.NewGuid(); 
        
            var newFolder = File.Create(
                rootFolderId,
                size: 0,
                name: "root",
                uploaderId: user.Id,
                uploadedAt: DateTime.UtcNow,
                fileTypeId: 8,
                isDirectory: true
            );

            newFolder.Owners = new List<User> { user };
            await _fileRepository.AddAsync(newFolder);

            user.RootDirectoryId = rootFolderId;
            await _userRepository.UpdateAsync(user, user.Id);

            return BaseApiResponse.Create("Successfully created user");
        }, "Error when creating user");
    }

    public async Task<AuthenticateUserResponseDto> AuthenticateUserAsync(AuthenticateUserDto authenticateUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.EnsureUserExistsAsync(authenticateUserDto.Email);
            await _entityValidator.ValidatePasswordAsync(authenticateUserDto.Password, user);

            var jwtToken = GenerateJwtToken(user);
            return AuthenticateUserResponseDto.Create("Successful authentication", jwtToken);
        }, "Error when authenticating user");
    }

    public async Task<GetUserResponseDto> GetUserAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            return GetUserResponseDto.Create("Successfully retrieved user", _mapper.Map<GetUserDto>(user));
        }, "Error when getting user");
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}