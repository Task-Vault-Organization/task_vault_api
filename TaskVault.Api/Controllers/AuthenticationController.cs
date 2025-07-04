﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.Authentication.Abstractions;
using TaskVault.Contracts.Features.Authentication.Dtos;
using System;
using System.Threading.Tasks;

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

    [HttpPost("google")]
    public async Task<IActionResult> AuthenticateUserGoogleAsync([FromBody] AuthenticateUserGoogleDto authenticateUserGoogleDto)
    {
        return Ok(await _authenticationService.AuthenticateUserGoogleAsync(authenticateUserGoogleDto));
    }

    [Authorize]
    [HttpGet("you")]
    public async Task<IActionResult> GetUserAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _authenticationService.GetUserAsync(userEmail));
    }

    [HttpGet("email-confirmation-request/{userId}")]
    public async Task<IActionResult> CheckEmailConfirmationRequestsAsync([FromRoute] Guid userId)
    {
        return Ok(await _authenticationService.CheckIfUserHasEmailConfirmationRequests(userId));
    }

    [HttpPost("email-confirmation-request/{userId}")]
    public async Task<IActionResult> CreateEmailConfirmationRequestAsync([FromRoute] Guid userId)
    {
        return Ok(await _authenticationService.CreateEmailConfirmationRequestForUser(userId));
    }

    [HttpPost("verify-email-confirmation/{userId}/{code}")]
    public async Task<IActionResult> VerifyEmailConfirmationCodeAsync([FromRoute] Guid userId, [FromRoute] string code)
    {
        return Ok(await _authenticationService.VerifyEmailConfirmationCodeAsync(userId, code));
    }
}
