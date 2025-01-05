namespace FuddyDuddy.Core.Domain.ValueObjects;

public record HealthCheck
{
    public DateTimeOffset LastCheck { get; }
    public HealthStatus Status { get; }
    public string? Message { get; }

    public HealthCheck(HealthStatus status, string? message = null)
    {
        LastCheck = DateTimeOffset.UtcNow;
        Status = status;
        Message = message;
    }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
} 