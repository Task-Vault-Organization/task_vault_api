using System.Text.Json;
using TaskVault.Contracts.Features.Llm.Dtos;

namespace TaskVault.Contracts.Features.Llm.Abstractions;

public interface ILlmService
{
    Task<CheckFileMatchesCategoryResponseDto> CheckIfFileMatchesCategoryAsync(string userEmail, CheckFileMatchesCategoryDto checkFileMatchesCategoryDto);
    Task<JsonDocument> ExtractStructuredJsonFromImageAsync(string userEmail, Guid fileId);
    Task<CategorizeFolderResponseDto> CategorizeFolderAsync(string userEmail, CategorizeFolderRequestDto dto);
}