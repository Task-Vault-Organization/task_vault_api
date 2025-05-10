using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.FileClassifierTrainer.Abstractions;
using TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

namespace TaskVault.Api.Controllers;

[Route("api/classifier-trainer")]
public class FileClassifierTrainerController : Controller
{
    private readonly IFileClassifierTrainer _fileClassifierTrainer;

    public FileClassifierTrainerController(IFileClassifierTrainer fileClassifierTrainer)
    {
        _fileClassifierTrainer = fileClassifierTrainer;
    }

    [HttpPost("train")]
    public async Task<IActionResult> TrainFileAsync([FromForm] TrainClassifierDto trainClassifierDto)
    {
        return Ok(await _fileClassifierTrainer.TrainFileClassifierModelOnFiles("", trainClassifierDto)); 
    }
    
    [HttpPost("trainnegative")]
    public async Task<IActionResult> TrainNegativeFileAsync([FromForm] TrainClassifierExtendedDto train)
    {
        return Ok(await _fileClassifierTrainer.RetrainModelWithAdditionalData("", train.Positive, train.Negative)); 
    }
    
    [HttpPost("predict")]
    public async Task<IActionResult> PredictFileAsync(IFormFile pdfFile)
    {
        return Ok(await _fileClassifierTrainer.PredictWithClassifier("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", pdfFile)); 
    }
}