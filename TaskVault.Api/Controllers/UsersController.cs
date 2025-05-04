using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Shared.Abstractions.Services;

namespace TaskVault.Api.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }
    
    [HttpGet("search/{searchField}")]
    public async Task<IActionResult> GetOwnedTasksAsync(string searchField)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _usersService.SearchUsersAsync(userEmail, searchField));
    }
}