using FuddyDuddy.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FuddyDuddy.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<INewsSourceDialectFactory, NewsSourceDialectFactory>();
        services.AddScoped<INewsProcessingService, NewsProcessingService>();
        services.AddScoped<ISummaryValidationService, SummaryValidationService>();
        services.AddScoped<ISummaryTranslationService, SummaryTranslationService>();
        services.AddScoped<IDigestCookService, DigestCookService>();

        return services;
    }
} 