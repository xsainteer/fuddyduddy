# Vector Search Implementation

## Overview
Adding vector search capabilities to FuddyDuddy enhances the search experience by enabling semantic search across news articles and summaries. This allows users to find relevant content even when exact keyword matches aren't present.

## Current System Analysis
- News articles are processed and summarized using AI
- Similarity detection is implemented using vector search
- Frontend has filtering capabilities (category, source, language, date range)
- Backend uses MySQL for storage and Qdrant for vector search

## Technical Design

### 1. Vector Database Selection
**Selected: Qdrant**
- High-performance vector similarity search engine
- Supports filtering with payload attributes
- Efficient real-time updates
- Supports multiple collections (one per language)
- Built-in rate limiting support
- Provides both gRPC and HTTP APIs

Alternatives considered:
- ChromaDB: Simpler but less scalable
- Pinecone: Proprietary, higher cost
- Weaviate: Good but more complex than needed

### 2. Architecture Implementation

#### 2.1 Core Domain Interfaces
```csharp
public interface IVectorSearchService
{
    Task IndexSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<(Guid SummaryId, float Score)>> SearchAsync(
        string query,
        Language language,
        DateTime? fromDate,
        DateTime? toDate,
        IEnumerable<int>? categoryIds,
        IEnumerable<Guid>? sourceIds,
        int limit = 10,
        CancellationToken cancellationToken = default);
    
    Task DeleteSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default);
    
    Task RecreateCollectionAsync(CancellationToken cancellationToken = default);
}
```

#### 2.2 Infrastructure Components

1. Vector Database Integration:
   - QdrantClient configuration
   - Collection management per language
   - Point operations (upsert, delete)
   - Search with filters

2. Embedding Service:
   - Integration with AI embedding models
   - Vector generation for queries and content
   - Caching and optimization

3. Search Service Features:
   - Language-specific collections
   - Date range filtering
   - Category and source filtering
   - Rate limiting implementation
   - Score-based result ranking

### 3. Implementation Details

#### Collection Management
```csharp
private async Task EnsureCollectionExists(Language language, CancellationToken cancellationToken)
{
    var collectionName = $"{_options.CollectionName}_{language.GetDescription()}";
    if (!await _qdrantClient.CollectionExistsAsync(collectionName))
    {
        await _qdrantClient.CreateCollectionAsync(
            collectionName,
            new VectorParams { 
                Size = (ulong)_options.VectorSize,
                Distance = _options.Distance,
                OnDisk = false
            });
    }
}
```

#### Point Structure
Each point in Qdrant contains:
- ID: Summary GUID
- Vector: Embedding of title and content
- Payload:
  - timestamp: Unix timestamp of generation
  - source: News source ID
  - category: Category ID

#### Search Implementation
- Supports complex filters:
  - Date range using timestamp payload
  - Category and source filtering
  - Language-specific collections
- Returns scored results ordered by similarity

### 4. Rate Limiting

The service implements rate limiting using Redis:
```csharp
private async Task CheckRateLimit(CancellationToken cancellationToken)
{
    if (_options.RatePerMinute > 0)
    {
        var waitTime = await _rateLimiter.GetMinuteTokenAsync(
            string.Format(LEAKY_BUCKET_KEY, _options.CollectionName),
            _options.RatePerMinute,
            120);
        // Handle rate limiting with wait or rejection
    }
}
```

### 5. Performance Considerations

1. Collection Design:
   - Separate collections per language
   - In-memory vector storage for speed
   - Payload-based filtering

2. Query Optimization:
   - Rate limiting to prevent overload
   - Efficient filter combinations
   - Limit result set size

3. Resource Management:
   - Connection pooling
   - Async operations
   - Error handling and logging

### 6. Monitoring and Maintenance

1. Logging Strategy:
   - Operation success/failure
   - Rate limit events
   - Collection management
   - Search performance

2. Maintenance Operations:
   - Collection recreation
   - Point deletion
   - Index optimization

### 7. Error Handling

The service implements comprehensive error handling:
- Collection existence checks
- Rate limit enforcement
- Summary existence validation
- Embedding generation errors
- Search operation failures

### 8. Future Improvements

1. Performance Enhancements:
   - Implement batch operations
   - Add vector caching
   - Optimize payload structure

2. Feature Additions:
   - Hybrid search (text + vector)
   - Dynamic vector size configuration
   - Advanced filtering options

3. Monitoring:
   - Add performance metrics
   - Implement health checks
   - Enhanced error tracking

4. Scalability:
   - Cluster support
   - Sharding strategy
   - Backup and recovery

## Next Steps
1. Set up Qdrant development environment
2. Create proof of concept with basic vector search
3. Review and refine the implementation plan
4. Begin Phase 1 implementation 
5. Implement hybrid search [https://qdrant.tech/documentation/beginner-tutorials/hybrid-search-fastembed/](https://qdrant.tech/documentation/beginner-tutorials/hybrid-search-fastembed/)