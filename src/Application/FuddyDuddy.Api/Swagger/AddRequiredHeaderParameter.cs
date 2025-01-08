using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace FuddyDuddy.Api.Swagger;

public class AddRequiredHeaderParameter : IOperationFilter
{
    private readonly ILogger<AddRequiredHeaderParameter> _logger;
    public AddRequiredHeaderParameter(ILogger<AddRequiredHeaderParameter> logger)
    {
        _logger = logger;
    }
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        _logger.LogInformation("Applying AddRequiredHeaderParameter filter {path}", context.ApiDescription.RelativePath);
        if (context?.ApiDescription?.RelativePath?.Contains("Maintenance") ?? false)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "api-key",
                In = ParameterLocation.Header,
                Description = "api-key for authorized operations",
                Required = true
            });
        }
    }
} 