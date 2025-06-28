using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskVault.Api;
using TaskVault.Api.Providers;
using TaskVault.Business;
using TaskVault.Business.Shared.Hubs;
using TaskVault.Business.Shared.Options;
using TaskVault.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((serverOptions) =>
{
    serverOptions.ListenAnyIP(5000);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

const string allowedSpecificOrigin = "AllowFlutterApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedSpecificOrigin,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Secret") ?? ""))
        };
        
    });
builder.Services.AddControllers();
builder.Services.AddBusinessLogic(builder.Configuration);
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddS3Storage();
builder.Services.AddGlobalErrorHandling();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Msa Cooking Application API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.Configure<AwsOptions>(builder.Configuration.GetSection("Aws"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUpload"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddHttpClient(builder.Configuration["ApiClients:Spoonacular:Name"] ?? "", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiClients:Spoonacular:BaseAddress"] ?? "");
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

var app = builder.Build();
app.UseCors(allowedSpecificOrigin);
if (builder.Environment.IsDevelopment() || builder.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Brackets.Play.API v1"));
}

app.UseGlobalErrorHandling();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();