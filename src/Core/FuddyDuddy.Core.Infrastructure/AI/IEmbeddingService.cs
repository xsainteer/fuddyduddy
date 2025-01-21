namespace FuddyDuddy.Core.Infrastructure.AI;

internal interface IEmbeddingService
{
    /// <summary>
    /// Generates embeddings for the given text
    /// </summary>
    /// <param name="text">Text to generate embeddings for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of float values representing the embedding vector</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts in batch
    /// </summary>
    /// <param name="texts">List of texts to generate embeddings for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of embedding vectors</returns>
    Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
} 