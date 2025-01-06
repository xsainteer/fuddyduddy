using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Infrastructure.Data.Repositories;
using FuddyDuddy.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FuddyDuddy.Core.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FuddyDuddyDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly(typeof(FuddyDuddyDbContext).Assembly.FullName)));

        services.AddScoped<INewsSourceRepository, NewsSourceRepository>();
        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<INewsSummaryRepository, NewsSummaryRepository>();

        return services;
    }
} 