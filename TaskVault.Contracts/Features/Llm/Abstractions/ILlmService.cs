using TaskVault.Contracts.Features.Llm.Dtos;

namespace TaskVault.Contracts.Features.Llm.Abstractions;

public interface ILlmService
{
    Task<CheckFileMatchesCategoryResponseDto> CheckIfFileMatchesCategoryAsync(string userEmail, CheckFileMatchesCategoryDto checkFileMatchesCategoryDto);
}