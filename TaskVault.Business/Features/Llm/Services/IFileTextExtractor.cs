namespace TaskVault.Business.Features.Llm.Services;

public interface IFileTextExtractor
{
    string ExtractText(byte[] fileBytes);
}