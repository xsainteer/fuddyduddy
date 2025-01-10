using System.Threading.Channels;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface IBroker : IDisposable
{
    /// <summary>
    /// Consumes messages from a specified queue
    /// </summary>
    /// <typeparam name="T">Type of the message</typeparam>
    /// <param name="queueName">Name of the queue to consume from</param>
    /// <param name="handler">Handler function to process the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConsumeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Pushes a message to a specified queue
    /// </summary>
    /// <typeparam name="T">Type of the message</typeparam>
    /// <param name="queueName">Name of the queue to push to</param>
    /// <param name="message">Message to push</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PushAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class;
} 