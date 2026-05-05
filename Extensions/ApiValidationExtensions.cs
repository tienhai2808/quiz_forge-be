using Microsoft.AspNetCore.Mvc;
using QuizForge.DTOs;

namespace QuizForge.Extensions;

public static class ApiValidationExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value is { Errors.Count: > 0 })
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors
                                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                                .ToArray()
                        );

                    var response = ApiResponseDto<Dictionary<string, string[]>>.Fail(
                        errors,
                        "Dữ liệu đầu vào không hợp lệ"
                    );

                    return new BadRequestObjectResult(response);
                };
            });

        return services;
    }
}