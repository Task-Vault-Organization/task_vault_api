using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.Authentication.Abstractions;
using TaskVault.Contracts.Features.Authentication.Dtos;

namespace TaskVault.Api.Controllers;

[Route("api/authenticate")]
public class AuthenticationController : Controller
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
    {
        return Ok(await _authenticationService.CreateUserAsync(createUserDto));
    }
    
    [HttpPost]
    public async Task<IActionResult> AuthenticateUserAsync([FromBody] AuthenticateUserDto authenticateUserDto)
    {
        return Ok(await _authenticationService.AuthenticateUserAsync(authenticateUserDto));
    }
    
    [Authorize]
    [HttpGet("you")]
    public async Task<IActionResult> GetUserAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _authenticationService.GetUserAsync(userEmail));
    }
}