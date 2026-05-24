using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using AppCorsOptions = QuizForge.Options.CorsOptions;

namespace QuizForge.Extensions;

public static class CorsExtensions
{
    private const string FrontendCorsPolicy = "FrontendPolicy";

    public static IServiceCollection AddAppCors(this IServiceCollection services)
    {
        services.AddCors();
        services
            .AddOptions<CorsOptions>()
            .Configure<IOptions<AppCorsOptions>>((corsOptions, appCorsOptions) =>
            {
                corsOptions.AddPolicy(FrontendCorsPolicy, policy =>
                    policy
                        .WithOrigins([.. appCorsOptions.Value.AllowedOrigins])
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

        return services;
    }

    public static IApplicationBuilder UseAppCors(this IApplicationBuilder app)
    {
        app.UseCors(FrontendCorsPolicy);
        return app;
    }
}
