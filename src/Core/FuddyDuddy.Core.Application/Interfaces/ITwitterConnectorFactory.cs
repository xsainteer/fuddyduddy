using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ITwitterConnectorFactory
{
    ITwitterConnector Create(Language language);
}