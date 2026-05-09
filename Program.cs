using Microsoft.EntityFrameworkCore;
using QuizForge.Data;
using QuizForge.Extensions;
using QuizForge.Middleware;
using QuizForge.Providers;
using QuizForge.Repositories;
using QuizForge.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiControllers();
builder.Services.AddOpenApi();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history")));
builder.Services.AddSnowflakeIdGenerator(builder.Configuration);
builder.Services.AddHttpClient<IOAuthProvider, GoogleOAuthProvider>();
builder.Services.AddScoped<IStorageProvider, GcsProvider>();
builder.Services.AddScoped<ITokenProvider, JwtProvider>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAppCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
