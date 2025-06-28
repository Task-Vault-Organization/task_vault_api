namespace TaskVault.Business.Features.Llm.Services;

public interface ILlmProvider
{
    Task<string> GenerateTextAsync(string prompt);
}