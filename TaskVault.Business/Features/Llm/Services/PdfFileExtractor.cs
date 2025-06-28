using System.Text;
using UglyToad.PdfPig;

namespace TaskVault.Business.Features.Llm.Services;

public class PdfFileExtractor : IFileTextExtractor
{
    private const int MaxCharacters = 10000;

    public string ExtractText(byte[] fileBytes)
    {
        using var pdf = PdfDocument.Open(new MemoryStream(fileBytes));
        var result = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            var text = page.Text;
            if (result.Length + text.Length > MaxCharacters)
            {
                int remaining = MaxCharacters - result.Length;
                result.Append(text.Substring(0, Math.Max(0, remaining)));
                break;
            }
            result.AppendLine(text);
        }

        return result.ToString();
    }
}