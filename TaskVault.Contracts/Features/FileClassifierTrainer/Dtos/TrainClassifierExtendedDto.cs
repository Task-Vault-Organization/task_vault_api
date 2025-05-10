namespace TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;

public class TrainClassifierExtendedDto
{
    public TrainClassifierDto Positive { get; set; }
    public TrainClassifierDto Negative { get; set; }
}