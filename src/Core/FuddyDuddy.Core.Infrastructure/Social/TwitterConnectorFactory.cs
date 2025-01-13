using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnectorFactory : ITwitterConnectorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TwitterConnectorFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITwitterConnector Create(Language language)
    {
        return ActivatorUtilities.CreateInstance<TwitterConnector>(_serviceProvider, language);
    }
}