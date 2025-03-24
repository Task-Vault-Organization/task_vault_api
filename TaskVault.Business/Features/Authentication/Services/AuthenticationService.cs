using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.Authentication.Abstractions;
using TaskVault.Contracts.Features.Authentication.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Repositories.Abstractions;
using User = TaskVault.DataAccess.Entities.User;

namespace TaskVault.Business.Features.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly JwtOptions _jwtOptions;

    public AuthenticationService(IExceptionHandlingService exceptionHandlingService, IRepository<User> userRepository, IOptions<JwtOptions> jwtOptions)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<BaseApiResponse> CreateUserAsync(CreateUserDto createUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync((u) => u.Email == createUserDto.Email)).FirstOrDefault();
            if (foundUser != null)
            {
                throw new ServiceException(StatusCodes.Status409Conflict, "Email already taken");
            }

            var createdUser = User.Create(createUserDto.Email, createUserDto.Password);
            HashUserPassword(createdUser);

            await _userRepository.AddAsync(createdUser);
            
            return BaseApiResponse.Create("Successfully created user");
        }, "Error when creating user");
    }

    public async Task<AuthenticateUserResponseDto> AuthenticateUserAsync(AuthenticateUserDto authenticateUserDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync((u) => u.Email == authenticateUserDto.Email)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            if (!VerifyHashedPassword(authenticateUserDto.Password, foundUser))
            {
                throw new ServiceException(StatusCodes.Status401Unauthorized, "Wrong credentials");
            }

            var jwtToken = GetJwtToken(foundUser);

            return AuthenticateUserResponseDto.Create("Successful authentication", jwtToken);
        }, "Error when authenticating user");
    }

    private void HashUserPassword(User user)
    {
        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        user.Password = passwordHasher.HashPassword(user, user.Password);
    }

    private bool VerifyHashedPassword(string passwordToVerify, User user)
    {
        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        return (passwordHasher.VerifyHashedPassword(user, user.Password, passwordToVerify)) ==
               PasswordVerificationResult.Success;
    }

    private string GetJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email)
        };
            
        var jwtSecret = _jwtOptions.Secret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = credentials
        };
            
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}