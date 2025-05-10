using Microsoft.AspNetCore.Http;

namespace TaskVault.Contracts.Features.FileClassifier.Dtos;

public class ClassifyFileDto
{
    public required string CategoryName { get; set; }
    public required IFormFile FileToClassify { get; set; }
}