using Microsoft.AspNetCore.Http;
using TaskVault.Business.Features.FileClassifierTrainer.Services;
using TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

namespace TaskVault.Contracts.Features.FileClassifierTrainer.Abstractions;

public interface IFileClassifierTrainer
{
    Task<TrainClassifierResponseDto> TrainFileClassifierModelOnFiles(string userEmail, TrainClassifierDto trainClassifierDto);

    Task<TrainClassifierResponseDto> RetrainModelWithAdditionalData(string userEmail, TrainClassifierDto positiveData,
        TrainClassifierDto negativeData);
    Task<PredictionResultDto> PredictFileCategory(string modelPath, IFormFile pdfFile);
    Task<PredictionResultDto> PredictWithClassifier(string apiKey, IFormFile pdfFile);
}