using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface IVectorSearchService
{
    /// <summary>
    /// Indexes a news summary for vector search
    /// </summary>
    /// <param name="summaryId">ID of the summary to index</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task IndexSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar summaries using vector similarity
    /// </summary>
    /// <param name="query">Search query text</param>
    /// <param name="language">Content language</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar summaries with their similarity scores</returns>
    Task<IEnumerable<(Guid SummaryId, float Score)>> SearchAsync(
        string query,
        Language language,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a summary from the vector index
    /// </summary>
    /// <param name="summaryId">ID of the summary to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default);
} 