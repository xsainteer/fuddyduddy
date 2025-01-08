using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Infrastructure.Data.Repositories;
using FuddyDuddy.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Cache;
using FuddyDuddy.Core.Infrastructure.AI;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Application;

namespace FuddyDuddy.Core.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Services");
        services.Configure<MySQLOptions>(section.GetSection("MySQL"));
        services.Configure<RedisOptions>(section.GetSection("Redis")); 
        services.Configure<GeminiOptions>(section.GetSection("Gemini"));
        services.Configure<OllamaOptions>(section.GetSection("Ollama"));
        // services.Configure<RabbitMQOptions>(section.GetSection("RabbitMQ"));

        var mysqlOptions = section.GetSection("MySQL").Get<MySQLOptions>() ?? throw new Exception("MySQL options are not configured");
        var redisOptions = section.GetSection("Redis").Get<RedisOptions>() ?? throw new Exception("Redis options are not configured");
        // var rabbitMQOptions = configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>() ?? throw new Exception("RabbitMQ options are not configured");
        var ollamaOptions = section.GetSection("Ollama").Get<OllamaOptions>() ?? throw new Exception("Ollama options are not configured");
        var geminiOptions = section.GetSection("Gemini").Get<GeminiOptions>() ?? throw new Exception("Gemini options are not configured");

        services.AddDbContext<FuddyDuddyDbContext>(options =>
            options.UseMySql(
                mysqlOptions.ConnectionString,
                ServerVersion.AutoDetect(mysqlOptions.ConnectionString),
                b => b.MigrationsAssembly(typeof(FuddyDuddyDbContext).Assembly.FullName)));

        services.AddScoped<INewsSourceRepository, NewsSourceRepository>();
        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<INewsSummaryRepository, NewsSummaryRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IDigestRepository, DigestRepository>();

        // Add Redis
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        // Register cache service
        services.AddScoped<ICacheService, RedisCacheService>();

        // Register AI service
        services.AddScoped<IAiService, GeminiAiService>();

        // Register http clients
        services.AddHttpClient(Constants.OLLAMA_HTTP_CLIENT_NAME, client =>
        {
            client.BaseAddress = new Uri(ollamaOptions.Url);
        });

        services.AddHttpClient(Constants.GEMINI_HTTP_CLIENT_NAME, client =>
        {
            client.BaseAddress = new Uri(geminiOptions.Url);
            client.DefaultRequestHeaders.Add("User-Agent", "FuddyDuddy/1.0 (News Digest Application)");
        });

        return services;
    }
} 