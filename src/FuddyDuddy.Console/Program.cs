using FuddyDuddy.Core.Application.Extensions;
using FuddyDuddy.Core.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Application.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(
    "Server=localhost;Database=fuddyduddy;User=fuddy;Password=duddy;");

// Build and run
using var host = builder.Build();

var processor = host.Services.GetRequiredService<NewsProcessingService>();
await processor.ProcessNewsSourcesAsync(); 