using FuddyDuddy.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using FuddyDuddy.Core.Application.Configuration;
using Microsoft.Extensions.Configuration;

namespace FuddyDuddy.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddTransient<INewsSourceDialectFactory, NewsSourceDialectFactory>();
        services.AddTransient<INewsProcessingService, NewsProcessingService>();
        services.AddTransient<ISummaryValidationService, SummaryValidationService>();
        services.AddTransient<ISummaryTranslationService, SummaryTranslationService>();
        services.AddTransient<IDigestCookService, DigestCookService>();
        services.AddTransient<ISimilarityService, SimilarityService>();
        
        // Maintenance
        services.AddTransient<IMaintenanceService, MaintenanceService>();

        // Business logic options
        services.Configure<ProcessingOptions>(configuration.GetSection(ProcessingOptions.SectionName));
        services.Configure<SimilaritySettings>(configuration.GetSection(SimilaritySettings.SectionName));
        return services;
    }
} 