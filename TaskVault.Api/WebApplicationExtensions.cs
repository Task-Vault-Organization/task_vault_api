namespace TaskVault.Api;

public static class WebApplicationExtensions
{
    public static WebApplication UseGlobalErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler("/error");
        return app;
    }
}