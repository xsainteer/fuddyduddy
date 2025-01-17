using System.Buffers;
using System.Text;
using System.Text.Json;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FuddyDuddy.Core.Infrastructure.Messaging;

public class RabbitMqBroker : IBroker
{
    private readonly IConnection _connection;
    private readonly ProducerPool _producerPool;
    private readonly ILogger<RabbitMqBroker> _logger;
    private bool _disposed;
    private const int MaxRetryAttempts = 10000; // Maximum number of retry attempts
    private const int LogFrequency = 100; // Log every 100th attempt
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() 
    { 
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqBroker(IConnectionFactory connectionFactory, ProducerPool producerPool, ILogger<RabbitMqBroker> logger)
    {
        _logger = logger;
        _connection = connectionFactory.CreateConnection();
        _producerPool = producerPool;
    }

    public async Task ConsumeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        var consumerChannel = _connection.CreateModel();
        try
        {
            var queueArgs = new Dictionary<string, object>{ {"x-queue-type", "quorum"} };
            consumerChannel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);

            var consumer = new AsyncEventingBasicConsumer(consumerChannel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var body = ea.Body.ToArray();
                    _logger.LogInformation("Received message from queue {QueueName}: {Message}", queueName, Encoding.UTF8.GetString(body));
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body), _jsonSerializerOptions);

                    if (message != null)
                    {
                        await handler(message);
                        consumerChannel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                    consumerChannel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            var consumerTag = consumerChannel.BasicConsume(queue: queueName,
                                                         autoAck: false,
                                                         consumer: consumer);

            // Wait for cancellation
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    consumerChannel.BasicCancel(consumerTag);
                    _logger.LogInformation("Consumer cancelled for queue {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling consumer for queue {QueueName}", queueName);
                }
            }
        }
        finally
        {
            if (consumerChannel?.IsOpen ?? false)
            {
                try
                {
                    consumerChannel.Close();
                    consumerChannel.Dispose();
                    _logger.LogInformation("Consumer cancelled for queue {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing consumer channel for queue {QueueName}", queueName);
                }
            }
        }
    }

    public Task PushAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        var attempt = 0;
        var sent = false;
        Exception? lastException = null;

        while (!sent && attempt < MaxRetryAttempts && !cancellationToken.IsCancellationRequested)
        {
            var channel = _producerPool.Rent();
            try
            {
                var queueArgs = new Dictionary<string, object>{ {"x-queue-type", "quorum"} };
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: string.Empty,
                                   routingKey: queueName,
                                   basicProperties: properties,
                                   body: body);
                
                sent = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt == 1 || attempt % LogFrequency == 0)
                {
                    _logger.LogError(ex, "Failed to publish message to queue {QueueName} on attempt {Attempt}", queueName, attempt);
                }

                // Add a small delay between retries
                Thread.Sleep(Math.Min(100 * attempt, 1000)); // Max 1 second delay
            }
            finally
            {
                _producerPool.Return(channel);
            }
        }

        if (!sent)
        {
            throw new InvalidOperationException($"Failed to publish message to queue {queueName} after {attempt} attempts", lastException);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connection?.Dispose();
        }

        _disposed = true;
    }
} 