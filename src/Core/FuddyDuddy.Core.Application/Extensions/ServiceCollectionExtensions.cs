using FuddyDuddy.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FuddyDuddy.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<NewsProcessingService>();
        services.AddScoped<SummaryValidationService>();
        services.AddScoped<SummaryTranslationService>();
        services.AddScoped<NewsSourceDialectFactory>();
        services.AddScoped<DigestCookService>();

        return services;
    }
} 