# Vector Search Implementation Plan

## Overview
Adding vector search capabilities to FuddyDuddy will enhance the search experience by enabling semantic search across news articles and summaries. This will allow users to find relevant content even when exact keyword matches aren't present.

## Current System Analysis
- News articles are processed and summarized using AI
- Similarity detection is already implemented using AI comparison
- Frontend has basic filtering capabilities (category, source, language)
- Backend uses MySQL for storage and Elasticsearch for text search

## Technical Design

### 1. Vector Database Selection
**Recommended: ChromaDB**
- Embedded vector database with built-in embedding support
- Simple deployment and maintenance
- HTTP API for easy integration
- Built-in metadata filtering and hybrid search
- Active development and community support
- Supports both in-memory and persistent storage

Alternatives considered:
- Qdrant: More complex but more scalable
- Pinecone: Proprietary, higher cost
- Weaviate: Good but more complex than needed

### 2. Architecture Changes

#### 2.1 Core Domain Changes
New entities and interfaces:
```csharp
public class VectorizedSummary
{
    public Guid Id { get; set; }
    public float[] Embedding { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Language { get; set; }
    public int? CategoryId { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public interface IVectorSearchService
{
    Task<IEnumerable<SearchResult>> SearchAsync(
        string query, 
        string language, 
        int? categoryId = null,
        int limit = 10,
        CancellationToken cancellationToken = default);
    
    Task IndexSummaryAsync(
        Guid summaryId,
        string content,
        string language,
        int? categoryId,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default);
}
```

#### 2.2 Infrastructure Changes
New components:
1. Vector Database Integration:
   - ChromaDB HTTP client configuration
   - Collection management
   - Vector operations service

2. Embedding Service:
   - Integration with Ollama running MXBai-Embed-Large
   - Local embedding generation
   - Batch processing capabilities
   - Failover handling

3. Search Service:
   - Hybrid search using ChromaDB's built-in capabilities
   - Result ranking and scoring
   - Filter integration via metadata

### 3. Implementation Phases

#### Phase 1: Infrastructure Setup
1. Add ChromaDB and Ollama containers to docker-compose:
```yaml
  chroma:
    image: chromadb/chroma:latest
    ports:
      - "8000:8000"
    volumes:
      - chroma_data:/chroma/data
    environment:
      - ALLOW_RESET=true
      - ANONYMIZED_TELEMETRY=false

  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      - ollama_data:/root/.ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
```

2. Implement ChromaDB integration
3. Set up Ollama with MXBai-Embed-Large model
4. Create initial collections with proper metadata schema

#### Phase 2: Core Implementation
1. Implement VectorSearchService
2. Add background job for indexing new summaries
3. Create API endpoints for vector search
4. Implement hybrid search combining Elasticsearch and vector search

#### Phase 3: Frontend Integration
1. Add search bar component
2. Implement search results page
3. Add search suggestions
4. Enhance filtering UI

#### Phase 4: Optimization
1. Implement embedding caching
2. Add batch processing for embeddings
3. Optimize search performance
4. Add monitoring and analytics

### 4. API Design

#### New Endpoints:
```
GET /api/search
Parameters:
- query (string): Search query
- language (string): Content language
- categoryId (int?): Optional category filter
- page (int): Page number
- pageSize (int): Results per page
- useVectorSearch (bool): Whether to use vector search

Response:
{
    "items": [
        {
            "id": "guid",
            "title": "string",
            "summary": "string",
            "score": float,
            "category": "string",
            "source": "string",
            "publishedAt": "datetime"
        }
    ],
    "totalCount": int,
    "hasMore": bool
}
```

### 5. Performance Considerations
1. Embedding Generation:
   - Batch process embeddings
   - Cache frequently used embeddings
   - Use background jobs for indexing

2. Search Performance:
   - Implement result caching
   - Use pagination
   - Optimize vector dimensions

3. Resource Usage:
   - Monitor memory usage
   - Implement connection pooling
   - Set up proper indexes

### 6. Monitoring and Maintenance
1. Metrics to track:
   - Search latency
   - Embedding generation time
   - Cache hit rates
   - Error rates

2. Maintenance tasks:
   - Regular index optimization
   - Embedding updates
   - Performance monitoring
   - Error tracking

### 7. Testing Strategy
1. Unit Tests:
   - Embedding generation
   - Search logic
   - Filter combinations

2. Integration Tests:
   - Vector database operations
   - API endpoints
   - Search accuracy

3. Performance Tests:
   - Search latency
   - Concurrent searches
   - Large result sets

### 8. Rollout Plan
1. Development Phase:
   - Set up development environment
   - Implement core features
   - Initial testing

2. Beta Testing:
   - Deploy to staging
   - Internal testing
   - Performance optimization

3. Production Release:
   - Gradual rollout
   - Monitor performance
   - Gather feedback

4. Post-Release:
   - Monitor usage
   - Collect metrics
   - Iterate based on feedback

## Next Steps
1. Set up Qdrant development environment
2. Create proof of concept with basic vector search
3. Review and refine the implementation plan
4. Begin Phase 1 implementation 