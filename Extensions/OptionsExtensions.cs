using QuizForge.Options;

namespace QuizForge.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddAppOptions(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<TokenOptions>()
            .BindConfiguration(TokenOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<GcsOptions>()
            .BindConfiguration(GcsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<SnowflakeOptions>()
            .BindConfiguration(SnowflakeOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<GoogleOAuthOptions>()
            .BindConfiguration(GoogleOAuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
