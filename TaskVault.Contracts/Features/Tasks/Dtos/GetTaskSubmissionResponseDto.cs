namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskSubmissionsResponseDto
{
    public string Message { get; set; }
    public IEnumerable<GetTaskSubmissionDto> Submissions { get; set; }

    private GetTaskSubmissionsResponseDto(string message, IEnumerable<GetTaskSubmissionDto> submissions)
    {
        Message = message;
        Submissions = submissions;
    }

    public static GetTaskSubmissionsResponseDto Create(string message, IEnumerable<GetTaskSubmissionDto> submissions)
    {
        return new GetTaskSubmissionsResponseDto(message, submissions);
    }
}
