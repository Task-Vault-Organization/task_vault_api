namespace TaskVault.Business.Shared.Options;

public class AwsOptions
{
    public const string Aws = "Aws";
    
    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}