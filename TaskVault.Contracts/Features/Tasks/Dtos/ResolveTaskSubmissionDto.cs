namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class ResolveTaskSubmissionDto
{
    public required Guid SubmissionId { get; set; }
    public required bool IsApproved { get; set; }
    public string? DissaproveComment { get; set; }
}