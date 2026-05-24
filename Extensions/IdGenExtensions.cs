using IdGen;
using Microsoft.Extensions.Options;
using QuizForge.Options;

namespace QuizForge.Extensions;

public static class IdGenExtensions
{
    public static IServiceCollection AddSnowflakeIdGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IIdGenerator<long>>(sp =>
        {
            var snowflakeOptions = sp.GetRequiredService<IOptions<SnowflakeOptions>>().Value;
            var idStructure = new IdStructure(41, 10, 12);
            var options = new IdGeneratorOptions(
                idStructure: idStructure,
                timeSource: new DefaultTimeSource(snowflakeOptions.Epoch)
            );

            return new IdGenerator(snowflakeOptions.GeneratorId, options);
        });

        return services;
    }
}
