using FuddyDuddy.Core.Application.Extensions;
using FuddyDuddy.Core.Infrastructure.Extensions;
using FuddyDuddy.Api.Middleware;

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
builder.Services.AddSwaggerGen(c => c.OperationFilter<AuthHeaderFilter>());

builder.Services.AddScoped<AuthMiddleware>();

// Add our application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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