namespace TaskVault.Business.Shared.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromHours(3);
}