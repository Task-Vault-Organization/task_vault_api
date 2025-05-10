using Microsoft.AspNetCore.Http;

namespace TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

public class TrainClassifierDto
{
    public required string Category { get; set; }
    public required IFormFile TrainDataSetZip { get; set; }
}