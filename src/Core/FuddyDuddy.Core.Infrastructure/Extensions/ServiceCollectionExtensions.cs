using FuddyDuddy.Core.Application.Repositories;
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
using FuddyDuddy.Core.Infrastructure.Http;
using System.Net;
using Polly;
using Polly.Extensions.Http;
using System.Net.Security;
using RabbitMQ.Client;
using FuddyDuddy.Core.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Constants;
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
        services.Configure<RabbitMqOptions>(section.GetSection("RabbitMQ"));

        var mysqlOptions = section.GetSection("MySQL").Get<MySQLOptions>() ?? throw new Exception("MySQL options are not configured");
        var redisOptions = section.GetSection("Redis").Get<RedisOptions>() ?? throw new Exception("Redis options are not configured");
        var ollamaOptions = section.GetSection("Ollama").Get<OllamaOptions>() ?? throw new Exception("Ollama options are not configured");
        var geminiOptions = section.GetSection("Gemini").Get<GeminiOptions>() ?? throw new Exception("Gemini options are not configured");
        
        // Crawler options
        services.Configure<CrawlerOptions>(configuration.GetSection("Crawler"));
        services.Configure<ProxyOptions>(configuration.GetSection("Proxy"));
        var crawlerOptions = configuration.GetSection("Crawler").Get<CrawlerOptions>() ?? throw new Exception("Crawler options are not configured");

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

        // Add RabbitMQ
        services.AddRabbitMq();

        // Register cache service
        services.AddScoped<ICacheService, RedisCacheService>();

        // Register AI service
        services.AddScoped<IGeminiService, GeminiAiService>();
        services.AddScoped<IOllamaService, OllamaAiService>();

        // Register crawler middleware
        services.AddSingleton<ICrawlerMiddleware, CrawlerMiddleware>();

        // Register proxy pool manager
        services.AddSingleton<IProxyPoolManager, ProxyPoolManager>();

        // Register http clients
        services.AddHttpClient(HttpClientConstants.OLLAMA, client =>
        {
            client.BaseAddress = new Uri(ollamaOptions.Url);
        });

        services.AddHttpClient(HttpClientConstants.GEMINI, client =>
        {
            client.BaseAddress = new Uri(geminiOptions.Url);
            client.DefaultRequestHeaders.Add("User-Agent", crawlerOptions.DefaultUserAgent);
        });

        services.AddHttpClient(HttpClientConstants.CRAWLER, client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", crawlerOptions.DefaultUserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new HttpClientHandler
            {
                UseProxy = crawlerOptions.UseProxies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => 
                    {
                        // Accept all certificates when using proxy
                        return crawlerOptions.UseProxies || errors == SslPolicyErrors.None;
                    }
            };

            if (crawlerOptions.UseProxies)
            {
                var proxyPool = sp.GetRequiredService<IProxyPoolManager>();
                var proxyAddress = proxyPool.GetNextProxy();
                if (proxyAddress != null)
                {
                    var proxyUri = new Uri(proxyAddress);
                    handler.Proxy = new WebProxy(proxyUri);
                }
                else
                {
                    handler.UseProxy = false;
                }
            }

            return handler;
        })
        .AddPolicyHandler(GetRetryPolicy())
        .ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", crawlerOptions.DefaultUserAgent);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddTransient<IConnectionFactory>(sp => {
            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                DispatchConsumersAsync = true,
                ConsumerDispatchConcurrency = 1,
                TopologyRecoveryEnabled = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(30)
            };

            return factory;
        });

        services.AddTransient<ProducerPool>();
        services.AddSingleton<IBroker, RabbitMqBroker>();
        return services;
    }
} 