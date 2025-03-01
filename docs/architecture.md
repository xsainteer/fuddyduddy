# Architecture Overview

Fuddy-Duddy follows Clean Architecture principles to maintain a flexible, testable, and maintainable codebase. This document outlines the high-level architecture and key design decisions.

## System Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │      API        │     │   Background    │
│    (React)      │◄───►│    (.NET 9)     │◄───►│    Workers      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                              │   │   │               │   │   │
                              │   │   │               │   │   │
                              ▼   ▼   ▼               ▼   ▼   ▼
┌──────────┐     ┌────────┐     ┌─────────┐     ┌────────┐     ┌────────┐
│  MySQL   │     │ Redis  │     │ RabbitMQ│     │ Qdrant │     │ MinIO  │
└──────────┘     └────────┘     └─────────┘     └────────┘     └────────┘
```

## Clean Architecture Layers

### 1. Domain Layer (Core)
- Contains business entities and logic
- No dependencies on external frameworks
- Pure C# code with domain rules
- Example: `NewsArticle`, `NewsSource`, `Category`

### 2. Application Layer
- Contains business use cases and application services
- Implements application logic and orchestration
- Depends only on the Domain layer
- Uses service-based architecture with dependency injection
- Example: `NewsProcessingService`, `DigestCookService`

### 3. Infrastructure Layer
- Implements interfaces from Domain layer
- Contains external service integrations
- Database access and external APIs
- Example: `NewsSummaryRepository`

### 4. API Layer
- ASP.NET Core Web API
- Handles HTTP requests
- Maps DTOs to domain models
- Minimal API endpoints

## Key Components

### News Collection
- Sitemap crawlers collect news articles
- Articles are stored in MySQL
- Vector embeddings are generated and stored in Qdrant
- Background workers process articles asynchronously

### AI Processing Pipeline
1. Article text extraction
2. Language detection
3. Text summarization (Ollama/Gemini)
4. Vector embedding generation
5. Similar article detection

### Caching Strategy
- Redis for application caching
- Distributed caching for API responses
- Cache invalidation through events

### Message Queue
- RabbitMQ for async communication
- Event-driven architecture
- Retry policies and dead letter queues

### Search and Recommendations
- Vector similarity search with Qdrant
- Hybrid search combining both approaches (needs to be implemented)

## Data Flow

1. News Collection:
   ```
   Crawler → RabbitMQ → Worker → MySQL + Qdrant
   ```

2. Article Processing:
   ```
   Worker → AI Service → RabbitMQ → Storage
   ```

3. User Requests:
   ```
   Client → API → Cache/DB → Response
   ```

## Security

- JWT authentication
- Role-based authorization
- API rate limiting
- Input validation
- HTTPS everywhere
- Secure configuration management

## Monitoring and Logging

- Structured logging with Serilog (needs to be implemented)
- Metrics collection (needs to be implemented)
- Distributed tracing (needs to be implemented)
- Health checks (needs to be implemented)
- Error tracking (needs to be implemented)

## Deployment

- Containerized with Docker
- Orchestrated with Kubernetes (maybe in the future)
- CI/CD with Azure Pipelines
- Blue-green deployments
- Automated backups

## Performance Considerations

- Async operations where possible
- Efficient caching strategies
- Database indexing
- Connection pooling
- Resource optimization

## Future Considerations

- Microservices architecture
- Event sourcing
- Real-time updates
- Enhanced AI capabilities
- Geographic distribution 