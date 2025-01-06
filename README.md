# FuddyDuddy News Aggregator

A news aggregation and summarization platform that collects news from various sources and generates AI-powered summaries.

## Architecture

### Current Implementation
- Collects news from source-specific sitemaps using dialect-based parsers
- Uses Ollama (owl/t-lite model) for news summarization
- Stores both raw news and summaries in MySQL
- Exposes REST API for triggering news collection and retrieval

### Future Enhancements
- Add Elasticsearch for full-text search capabilities
- Implement RabbitMQ for async processing
- Add Redis caching
- Add MinIO for storing full article content
- Implement news digest generation

## Data Flow
1. News Collection
   - Fetch sitemaps from configured sources
   - Parse using source-specific dialects
   - Store raw news content

2. News Processing
   - Extract relevant content
   - Generate AI summaries
   - Store structured summaries

3. Data Storage (MySQL)
   - News sources configuration
   - Raw news articles
   - AI-generated summaries with metadata

## Tech Stack
- .NET 9
- MySQL
- Ollama AI
- Clean Architecture
- CQRS pattern

