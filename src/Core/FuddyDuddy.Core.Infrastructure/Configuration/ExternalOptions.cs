namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class RedisOptions
{
    public string ConnectionString { get; set; }
}

public class MySQLOptions
{
    public string ConnectionString { get; set; }
}

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort PrefetchSize { get; set; } = 20;
} 