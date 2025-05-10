using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

public class TrainClassifierResponseDto : BaseApiResponse
{
    public ModelMetricsDto Metrics { get; set; }
}