using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace FuddyDuddy.Core.Infrastructure.Messaging;

internal class ProducerPool
{
    private readonly ObjectPool<IModel> _pool;
    private readonly ILogger<ProducerPool> _logger;

    public ProducerPool(IConnectionFactory connectionFactory, ILogger<ProducerPool> logger)
    {
        _logger = logger;

        var provider = new DefaultObjectPoolProvider();
        _pool = provider.Create(new ProducerPoolPolicy(connectionFactory));
    }

    public IModel Rent()
    {
        var channel = _pool.Get();
        _logger.LogDebug("Rented channel from pool {ChannelId}", channel.ChannelNumber);
        return channel;
    }

    public void Return(IModel channel)
    {
        _logger.LogDebug("Returned channel to pool {ChannelId}", channel.ChannelNumber);
        _pool.Return(channel);
    }

    class ProducerPoolPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly IConnection _connection;

        public ProducerPoolPolicy(IConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();
        }

        public IModel Create()
        {
            var channel = _connection.CreateModel();
            return channel;
        }

        public bool Return(IModel obj)
        {
            return obj.IsOpen;
        }
    }
}
