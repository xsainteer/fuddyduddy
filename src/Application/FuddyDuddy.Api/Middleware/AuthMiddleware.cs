namespace FuddyDuddy.Api.Middleware;

public class AuthMiddleware : IMiddleware
{
    private readonly string _authSecret;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(IConfiguration configuration, ILogger<AuthMiddleware> logger)
    {
        _authSecret = configuration["Auth:Secret"];
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/api/maintenance"))
        {
            var token = context.Request.Headers["api-key"].FirstOrDefault();
            if (token != _authSecret)
            {
                _logger.LogWarning("Unauthorized request to maintenance endpoint. Path: {Path}, Token: {Token}, Ip: {Ip}", context.Request.Path, token, context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}