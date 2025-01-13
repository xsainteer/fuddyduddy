namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class OllamaOptions
{
    public string Url { get; set; }
    public string Model { get; set; }
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
}

public class RabbitMQOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class GeminiOptions
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public string Url { get; set; }
}

public class GeminiSmartOptions
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public string Url { get; set; }
}

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
} 