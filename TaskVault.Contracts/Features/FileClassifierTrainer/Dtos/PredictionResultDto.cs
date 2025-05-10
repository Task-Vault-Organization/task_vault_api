namespace TaskVault.Business.Features.FileClassifierTrainer.Services;

public class PredictionResultDto
{
    public bool Success { get; set; }
    public string? PredictedCategory { get; set; }
    public double Probability { get; set; }
    public Dictionary<string, float>? Scores { get; set; }
    public string? ErrorMessage { get; set; }
}