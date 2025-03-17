using Amazon.S3;

namespace TaskVault.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        return services;
    }

    public static IServiceCollection AddS3Storage(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new AmazonS3Client(
                config["Aws:AccessKey"],
                config["Aws:SecretKey"],
                new AmazonS3Config
                {
                    ServiceURL = config["AWS:ServiceUrl"],
                    ForcePathStyle = true
                }
            );
        });
        return services;
    }
}