using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FuddyDuddy.Api.Middleware;

public class AuthHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters =
        [
            new OpenApiParameter
            {
                Name = "api-key",
                In = ParameterLocation.Header,
                Description = "api-key for authorized operations",
            },
        ];
    }
}