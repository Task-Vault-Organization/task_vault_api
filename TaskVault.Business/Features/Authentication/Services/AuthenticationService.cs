using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskVault.Business.Features.FileStorage.Services;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.Authentication.Abstractions;
using TaskVault.Contracts.Features.Authentication.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Features.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly IMapper _mapper;
    private readonly IEntityValidator _entityValidator;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<AuthenticationService> _logger;
    private const int RootFolderFileTypeId = 8;
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _passwordHasher;
    private readonly IEmailConfirmationRequestRepository _emailConfirmationRequestRepository;

    public AuthenticationService(
        IExceptionHandlingService exceptionHandlingService,
        IRepository<User> userRepository,
        IOptions<JwtOptions> jwtOptions,
        IMapper mapper,
        IEntityValidator entityValidator,
        IFileRepository fileRepository, ILogger<AuthenticationService> logger, IEmailConfirmationRequestRepository emailConfirmationRequestRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
        _entityValidator = entityValidator;
        _fileRepository = fileRepository;
        _logger = logger;
        _emailConfirmationRequestRepository = emailConfirmationRequestRepository;
        _passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
    }

    public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var existingUser = (await _userRepository.FindAsync(u => u.Email == createUserDto.Email)).FirstOrDefault();

            if (existingUser != null)
            {
                if (existingUser.GoogleId != null)
                {
                    throw new ServiceException(StatusCodes.Status409Conflict, "This email is registered with Google. Please sign in using Google.");
                }

                throw new ServiceException(StatusCodes.Status409Conflict, "A user with this email already exists.");
            }

            var user = User.Create(createUserDto.Email, createUserDto.FullName, false, createUserDto.Password, null);
            SetHashedPassword(user);

            await _userRepository.AddAsync(user);
            await CreateRootDirectoryForUser(user);
            await _userRepository.UpdateAsync(user, user.Id);

            await CreateEmailConfirmationAsync(user.Id);

            return CreateUserResponseDto.Create("Successfully created user", user.Id);
        }, "Error when creating user");
    }

    private async Task CreateEmailConfirmationAsync(Guid userId)
    {
        try
        {
            var expirationTime = DateTime.Now.AddMinutes(15);
            var confirmationCode = GenerateRandomCode(5);

            var newEmailConfirmationRequest = EmailConfirmationRequest.Create(
                userId,
                expirationTime,
                false,
                confirmationCode
            );

            await _emailConfirmationRequestRepository.AddAsync(newEmailConfirmationRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when creating email confirmation request");
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    public async Task<AuthenticateUserResponseDto> AuthenticateUserAsync(AuthenticateUserDto authenticateUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.EnsureUserExistsAsync(authenticateUserDto.Email);

            if (user.GoogleId != null)
            {
                throw new ServiceException(StatusCodes.Status409Conflict, "This email is registered via Google. Please sign in using Google.");
            }

            await _entityValidator.ValidatePasswordAsync(authenticateUserDto.Password, user);
            
            if (!user.EmailConfirmed)
                return AuthenticateUserResponseDto.Create("You need to confirm your email before proceding", "", false, user.Id);

            var jwtToken = GenerateJwtToken(user);
            return AuthenticateUserResponseDto.Create("Successful authentication", jwtToken, true, user.Id);
        }, "Error when authenticating user");
    }

    public async Task<AuthenticateUserResponseDto> AuthenticateUserGoogleAsync(AuthenticateUserGoogleDto authenticateUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(authenticateUserDto.AccessToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "619168613236-kgk2u0ihd6dkrimkfg3j5un8bn7um2p2.apps.googleusercontent.com" }
            });

            var user = (await _userRepository.FindAsync(u => u.Email == payload.Email)).FirstOrDefault();

            if (user != null)
            {
                if (user.GoogleId == null)
                {
                    throw new ServiceException(StatusCodes.Status409Conflict, "This email is already registered with a password. Please log in manually.");
                }
            }
            else
            {
                var generatedPassword = Guid.NewGuid().ToString("N");

                user = User.Create(
                    email: payload.Email,
                    fullName: payload.Name,
                    emailConfirmed: true,
                    password: generatedPassword,
                    payload.Subject
                );

                SetHashedPassword(user);

                await _userRepository.AddAsync(user);
                await CreateRootDirectoryForUser(user);
                await _userRepository.UpdateAsync(user, user.Id);
            }

            var jwtToken = GenerateJwtToken(user);
            return AuthenticateUserResponseDto.Create("Google login successful", jwtToken, true, user.Id);
        }, "Error when authenticating with Google");
    }

    public async Task<GetUserResponseDto> GetUserAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var mappedUser = _mapper.Map<GetUserDto>(user);
            return GetUserResponseDto.Create("Successfully retrieved user", mappedUser);
        }, "Error when getting user");
    }

    public async Task<CheckIfUserHasEmailConfirmationRequestsResponse> CheckIfUserHasEmailConfirmationRequests(Guid userId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var emailConfirmationRequests = (await _emailConfirmationRequestRepository.FindAsync((ecr) =>
                ecr.UserId == user.Id && !ecr.Confirmed && ecr.ExpiresAt > DateTime.Now));

            if (emailConfirmationRequests.Any())
            {
                return CheckIfUserHasEmailConfirmationRequestsResponse.Create("Found email confirmation requests",
                    true);
            }
            
            return CheckIfUserHasEmailConfirmationRequestsResponse.Create("Not found email confirmation requests",
                false);
        }, "Error when checking for email confirmation requests");
    }
    
    public async Task<BaseApiResponse> CreateEmailConfirmationRequestForUser(Guid userId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new ServiceException(StatusCodes.Status409Conflict, "Email already confirmed");
            }

            var emailConfirmationRequests = (await _emailConfirmationRequestRepository.FindAsync((ecr) =>
                ecr.UserId == user.Id && !ecr.Confirmed && ecr.ExpiresAt > DateTime.Now));

            if (emailConfirmationRequests.Any())
            {
                throw new ServiceException(StatusCodes.Status409Conflict, "Already have a pending email confirmation request");
            }

            await CreateEmailConfirmationAsync(user.Id);

            return BaseApiResponse.Create("Succesfully created email cofirmation request");
        }, "Error when creating email confirmation request");
    }
    
    public async Task<BaseApiResponse> VerifyEmailConfirmationCodeAsync(Guid userId, string code)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new ServiceException(StatusCodes.Status409Conflict, "Email already confirmed");
            }

            var emailConfirmationRequests = (await _emailConfirmationRequestRepository.FindAsync((ecr) =>
                ecr.UserId == user.Id && !ecr.Confirmed && ecr.ExpiresAt > DateTime.Now));

            var confirmationRequests = emailConfirmationRequests as EmailConfirmationRequest[] ?? emailConfirmationRequests.ToArray();
            if (!confirmationRequests.Any())
            {
                throw new ServiceException(StatusCodes.Status400BadRequest, "You have no incoming confirmation codes");
            }

            var emailConfirmation = confirmationRequests.First();

            if (emailConfirmation.CodeToVerify != code)
            {
                throw new ServiceException(StatusCodes.Status400BadRequest, "Invalid code");
            }

            user.EmailConfirmed = true;
            await _userRepository.UpdateAsync(user, user.Id);

            emailConfirmation.Confirmed = true;
            await _emailConfirmationRequestRepository.UpdateAsync(emailConfirmation, emailConfirmation.Id);
            
            return BaseApiResponse.Create("Succesfully verified confirmation code");
        }, "Error when checking for email confirmation code");
    }

    private void SetHashedPassword(User user)
    {
        user.Password = _passwordHasher.HashPassword(user, user.Password);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtOptions.TokenExpiry),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private async Task CreateRootDirectoryForUser(User user)
    {
        try
        { 
            var rootFolderId = Guid.NewGuid();

            var rootFolder = File.Create(
                rootFolderId,
                size: 0,
                name: "root",
                uploaderId: user.Id,
                uploadedAt: DateTime.UtcNow,
                fileTypeId: RootFolderFileTypeId,
                isDirectory: true
            );

            rootFolder.Owners = new List<User> { user };
            await _fileRepository.AddAsync(rootFolder);

            user.RootDirectoryId = rootFolderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory entry");
        }
    }
}