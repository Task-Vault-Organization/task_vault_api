namespace TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

public class ModelMetricsDto
{
    public double Accuracy { get; set; }
    public double Auc { get; set; }
    public double F1Score { get; set; }
}