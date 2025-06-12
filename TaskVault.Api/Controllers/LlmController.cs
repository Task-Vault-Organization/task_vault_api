using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.Llm.Abstractions;
using TaskVault.Contracts.Features.Llm.Dtos;

namespace TaskVault.Api.Controllers;

[Route("api/llm")]
[Authorize]
public class LlmController : Controller
{
    private readonly ILlmService _llmService;

    public LlmController(ILlmService llmService)
    {
        _llmService = llmService;
    }

    [HttpPost("check-file-category")]
    public async Task<IActionResult> CheckFileMatchesCategoryAsync([FromBody] CheckFileMatchesCategoryDto dto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        var result = await _llmService.CheckIfFileMatchesCategoryAsync(userEmail, dto);
        return Ok(result);
    }
}