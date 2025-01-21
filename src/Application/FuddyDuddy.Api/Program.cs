using FuddyDuddy.Core.Application.Extensions;
using FuddyDuddy.Core.Infrastructure.Extensions;
using FuddyDuddy.Api.Middleware;
using FuddyDuddy.Api.Swagger;
using FuddyDuddy.Api.HostedServices;
using FuddyDuddy.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? throw new Exception("AllowedOrigins are not configured");
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.OperationFilter<AddRequiredHeaderParameter>());

builder.Services.AddTransient<AuthMiddleware>();

// Add our application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add task scheduler service
builder.Services.AddHostedService<SimpleTaskScheduler>();

// Add listeners
builder.Services.AddHostedService<SimilarRequestListener>();
builder.Services.AddHostedService<IndexRequestListener>();

// Register task scheduler settings
builder.Services.Configure<TaskSchedulerSettings>(builder.Configuration.GetSection("TaskScheduler"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowAllOrigins");

// Use auth for maintenance endpoint
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.Run();