using Microsoft.Extensions.Options;
using QuizForge.Options;
using StackExchange.Redis;

namespace QuizForge.Extensions;

public static class CacheExtensions
{
    public static IServiceCollection AddAppCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        });

        return services;
    }
}
