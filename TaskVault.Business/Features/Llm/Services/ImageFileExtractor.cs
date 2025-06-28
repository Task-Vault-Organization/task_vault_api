using Tesseract;

namespace TaskVault.Business.Features.Llm.Services;

public class ImageFileExtractor : IFileTextExtractor
{
    public string ExtractText(byte[] fileBytes)
    {
        var tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
        using var engine = new TesseractEngine(tessdataPath, "ron+eng", EngineMode.Default);
        using var img = Pix.LoadFromMemory(fileBytes);
        using var page = engine.Process(img);
        return page.GetText();
    }
}