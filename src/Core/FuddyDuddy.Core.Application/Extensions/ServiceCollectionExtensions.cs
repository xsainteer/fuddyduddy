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
        services.AddScoped<INewsSourceDialectFactory, NewsSourceDialectFactory>();
        services.AddScoped<INewsProcessingService, NewsProcessingService>();
        services.AddScoped<ISummaryValidationService, SummaryValidationService>();
        services.AddScoped<ISummaryTranslationService, SummaryTranslationService>();
        services.AddScoped<IDigestCookService, DigestCookService>();
        
        // Maintenance
        services.AddScoped<IMaintenanceService, MaintenanceService>();

        // Business logic options
        services.Configure<ProcessingOptions>(configuration.GetSection(ProcessingOptions.SectionName));

        return services;
    }
} 