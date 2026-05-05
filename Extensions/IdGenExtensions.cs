using IdGen;

namespace QuizForge.Extensions;

public static class IdGenExtensions
{
    public static IServiceCollection AddSnowflakeIdGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Snowflake");
        int generatorId = section.GetValue("GeneratorId", 0);
        DateTime epochDate = section.GetValue("Epoch", new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc));

        var idStructure = new IdStructure(41, 10, 12);
        var options = new IdGeneratorOptions(
            idStructure: idStructure,
            timeSource: new DefaultTimeSource(epochDate)
        );

        var generator = new IdGenerator(generatorId, options);

        services.AddSingleton<IIdGenerator<long>>(generator);

        return services;
    }
}