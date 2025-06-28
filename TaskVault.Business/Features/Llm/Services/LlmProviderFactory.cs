using Microsoft.Extensions.Configuration;

namespace TaskVault.Business.Features.Llm.Services;

public class LlmProviderFactory
{
    public static ILlmProvider Create(IConfiguration config)
    {
        var provider = config["Llm:Provider"];
        return provider switch
        {
            "OpenAI" => new OpenAiLlmProvider(config),
            "Ollama" => new OllamaLlmProvider(config),
            _ => throw new NotSupportedException($"LLM provider '{provider}' is not supported.")
        };
    }
}
