using Qdrant.Client.Grpc;

namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class QdrantOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public string CollectionName { get; set; } = "news_summaries";
    public int VectorSize { get; set; } = 384; // Default size for multilingual-e5-large
    public int RatePerMinute { get; set; } = 0;
    public bool ExtractDateRange { get; set; } = false;
    public Distance Distance { get; set; } = Distance.Cosine;
}