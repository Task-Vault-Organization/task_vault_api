using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TaskVault.Business.Features.Llm.Services;

public class OllamaLlmProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _url;

    public OllamaLlmProvider(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _url = config["Llm:OllamaApiUrl"];
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        var payload = new
        {
            model = "mistral",
            prompt = prompt,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("response").GetString();
    }
}