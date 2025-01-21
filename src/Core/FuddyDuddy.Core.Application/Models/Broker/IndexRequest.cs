namespace FuddyDuddy.Core.Application.Models.Broker;

public record IndexRequest(Guid NewsSummaryId, IndexRequestType Type = IndexRequestType.Add);

public enum IndexRequestType
{
    Add,
    Delete
}