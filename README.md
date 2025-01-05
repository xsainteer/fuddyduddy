# 🗞️ Fuddy-Duddy

A smart news digest generator that collects, analyzes, and summarizes news from various media sources into concise, thoughtful social media posts.

## 🎯 Project Overview

Fuddy-Duddy is an automated news aggregation and summarization platform that:
- Collects news articles from multiple sources over the last 24 hours
- Analyzes and processes the content using AI to extract key information
- Generates concise, engaging summaries
- Publishes digests to various social media platforms (Twitter, Instagram, Facebook)
- Presents full digests in a clean, Medium-style web interface

## 🏗️ Architecture

The project is split into two main parts:

### Core (Reusable Library)
- News collection and processing pipeline
- AI abstraction layer with provider-agnostic interfaces
- Message bus integration
- Data persistence layer
- Common utilities and interfaces
- Resilience policies:
  - Circuit breakers for external services
  - Retry policies for transient failures
  - Rate limiting for API calls
  - Fallback strategies

### Application
- Web API
- Frontend interface
- Social media integration
- Digest presentation and management
- Platform-specific implementations

## 🔧 Tech Stack

### Backend
- .NET 9 (Latest)
- RabbitMQ for async communication
- MySQL for relational data
- Elasticsearch for search and analytics
- Redis for caching:
  - Digest caching
  - API response caching
  - Rate limiting data
  - Temporary processing results
- S3-compatible Object Storage:
  - Production: AWS S3
  - Local: MinIO
  - Use cases:
    - Raw article content storage
    - Media assets (images, videos)
    - Backup storage
    - Export files
- AI Provider Support:
  - Local: Ollama
  - Cloud: OpenAI, Anthropic Claude, Google Gemini, Grok, Ollama
  - Pluggable architecture for new AI providers
  - Features:
    - Text summarization
    - Content structuring
    - Translation services
    - Semantic analysis
- Monitoring & Logging:
  - Prometheus for metrics
  - Grafana for visualization
  - OpenTelemetry for tracing
  - Structured logging with Seq

### Frontend
- Minimalist, content-focused design
- Medium-style article presentation
- Responsive layout for all devices

## 🔄 Data Flow

1. News Collection
   - Source discovery and validation:
     ```json
     {
       "source_id": "unique-id",
       "domain": "example.com",
       "robots_txt": {
         "url": "https://example.com/robots.txt",
         "sitemaps": [
           "https://example.com/sitemap.xml",
           "https://example.com/news-sitemap.xml"
         ],
         "crawl_delay": 1,
         "rules": [
           {
             "user_agent": "*",
             "allow": ["/news/", "/articles/"],
             "disallow": ["/private/", "/admin/"]
           }
         ]
       },
       "active_sitemaps": [
         {
           "url": "https://example.com/news-sitemap.xml",
           "type": "news",
           "last_fetch": "2024-03-20T10:00:00Z",
           "update_frequency": "hourly"
         }
       ],
       "status": "active",
       "health_check": {
         "last_check": "2024-03-20T10:00:00Z",
         "status": "healthy"
       }
     }
     ```
   - Robots.txt compliance:
     - Parse robots.txt for sitemap locations
     - Extract crawl delays and rate limits
     - Follow allow/disallow rules
     - Respect crawl-delay directives
   - Sitemap handling:
     - Multi-strategy sitemap parsing:
       ```json
       {
         "sitemap_strategies": {
           "standard": {
             "type": "xml",
             "schema": "http://www.sitemaps.org/schemas/sitemap/0.9",
             "parser": "StandardSitemapParser"
           },
           "news": {
             "type": "xml",
             "schema": "http://www.google.com/schemas/sitemap-news/0.9",
             "parser": "NewsSitemapParser"
           },
           "pattern_based": {
             "patterns": [
               {
                 "name": "date_based",
                 "example": "sitemap_article.xml?date_start=20240320",
                 "regex": "date_start=(\\d{8})",
                 "parser": "DateBasedSitemapParser"
               },
               {
                 "name": "yearly",
                 "example": "sitemap_2024.xml",
                 "regex": "sitemap_(\\d{4})\\.xml",
                 "parser": "YearlySitemapParser"
               }
             ]
           },
           "fallback": {
             "type": "ai_assisted",
             "steps": [
               {
                 "action": "structure_detection",
                 "model": "local/small",
                 "prompt_template": "analyze_sitemap_structure"
               },
               {
                 "action": "pattern_extraction",
                 "model": "local/small",
                 "prompt_template": "extract_sitemap_pattern"
               }
             ]
           }
         }
       }
       ```
     - Processing pipeline:
       1. Try standard parsers first (XML-based)
       2. Apply pattern matching for known formats
       3. Use AI fallback for unknown structures
       4. Cache successful parsing strategies
     - Smart sitemap traversal:
       1. Parse index sitemap
       2. Extract date patterns
       3. Generate relevant date-based URLs
       4. Fetch only required date ranges
     - Efficient date filtering:
       - Only fetch sitemaps for last 24h
       - Cache sitemap index structure
       - Track last successful fetch
   - Intelligent URL extraction and deduplication
   - Timestamp-based filtering for last 24h content
   - Rate-limited content scraping with respect to robots.txt
   - Content persistence:
     - Raw HTML storage in S3
     - Media asset archival
     - Version tracking
   - AI fallback analysis result:
     ```json
     {
       "analysis_result": {
         "request_id": "unique-analysis-id",
         "source_url": "https://example.com/custom-sitemap.xml",
         "sample_content": "...(truncated content)...",
         "detected_structure": {
           "type": "custom",
           "format": "non_standard_xml",
           "nesting_levels": 2,
           "identified_patterns": [
             {
               "pattern_type": "temporal",
               "example": "/archive/2024/03/news.xml",
               "extracted_regex": "\\/archive\\/(\\d{4})\\/(\\d{2})\\/news\\.xml",
               "confidence": 0.95
             }
           ],
           "field_mapping": {
             "url": "//custom:entry/custom:link",
             "date": "//custom:entry/custom:published",
             "title": "//custom:entry/custom:headline"
           }
         },
         "recommended_strategy": {
           "parser_type": "custom_xml",
           "xpath_queries": {
             "urls": "//custom:entry/custom:link/text()",
             "dates": "//custom:entry/custom:published/text()",
             "titles": "//custom:entry/custom:headline/text()"
           },
           "namespaces": {
             "custom": "http://example.com/custom-news-schema"
           },
           "date_format": "yyyy-MM-dd'T'HH:mm:ssZ"
         },
         "validation": {
           "test_urls": [
             "https://example.com/article1",
             "https://example.com/article2"
           ],
           "success_rate": 0.98,
           "errors": []
         }
       },
       "strategy_cache": {
         "cache_key": "example.com_sitemap_strategy",
         "expires_at": "2024-03-21T00:00:00Z",
         "version": 1
       }
     }
     ```

     - Fallback processing steps:
       1. Initial structure analysis
          - Detect format type
          - Identify patterns
          - Extract field mappings
       2. Strategy generation
          - Create parsing rules
          - Define field mappings
          - Generate validation tests
       3. Validation and testing
          - Test on sample URLs
          - Verify data extraction
          - Calculate confidence score
       4. Strategy persistence
          - Cache successful strategies
          - Track version history
          - Set expiration policy

     - Error handling:
       ```json
       {
         "error_response": {
           "source_url": "https://example.com/sitemap.xml",
           "error_type": "parsing_failed",
           "attempts": [
             {
               "strategy": "standard_xml",
               "error": "Invalid XML format"
             },
             {
               "strategy": "pattern_matching",
               "error": "No matching patterns found"
             },
             {
               "strategy": "ai_fallback",
               "result": "success",
               "confidence": 0.95
             }
           ],
           "fallback_action": "use_ai_strategy",
           "retry_policy": {
             "next_attempt": "2024-03-20T11:00:00Z",
             "backoff_minutes": 30
           }
         }
       }
       ```

2. Processing Pipeline
   - Content extraction and cleaning
   - Content chunking and processing:
     ```json
     {
       "article_id": "unique-id",
       "original_length": 25000,
       "chunks": [
         {
           "sequence": 1,
           "content": "First 8k chunk of text...",
           "summary": "Summary of first chunk..."
         },
         {
           "sequence": 2,
           "content": "Next 8k chunk of text...",
           "summary": "Summary of second chunk..."
         }
       ],
       "intermediate_summary": "Combined summary of all chunks",
       "final_summary": "Concise article summary",
       "metadata": {
         "title": "Article Title",
         "source": "Source Name",
         "timestamp": "2025-01-03T17:00:00+06:00"
       }
     }
     ```
   - Hierarchical summarization process:
     1. Split content into optimal chunks (considering context overlap)
     2. Summarize each chunk independently
     3. Combine chunk summaries with context
     4. Generate final summary from intermediate summaries
   - Content versioning for change detection
   - Multi-language support with translation
   - Source credibility scoring
   - Cross-source analysis:
     - Semantic similarity detection
     - Story clustering and grouping
     - Narrative comparison
     - Source bias detection
     - Fact cross-verification

3. Digest Generation
   - Topic clustering and story grouping
   - Multi-source story aggregation:
     ```json
     {
       "story_id": "unique-id",
       "main_title": "Consolidated title",
       "summary": "Combined summary from multiple sources",
       "sources": [
         {
           "url": "https://source1.com/article",
           "source_name": "Source 1",
           "unique_details": "Additional details specific to this source..."
         },
         {
           "url": "https://source2.com/article",
           "source_name": "Source 2",
           "unique_details": "Different perspective or details..."
         }
       ],
       "importance_score": 0.85,
       "coverage_breadth": 3  // number of sources covering this story
     }
     ```
   - Importance ranking based on:
     - Coverage breadth
     - Source credibility
     - Social impact
     - Time relevance
   - Summary compilation with source attribution
   - Social media format adaptation

4. Publication
   - Automated social media posting
   - Web interface updates
   - Archive management
   - Cache management:
     - Digest caching with TTL
     - Cache invalidation on updates
     - Stale-while-revalidate strategy
     - Cache warming for popular digests

## 🚀 Development Roadmap

### Phase 1: Core Infrastructure
- [ ] Set up basic project structure
- [ ] Implement RabbitMQ integration
- [ ] Configure database schemas
- [ ] Establish Elasticsearch indices
- [ ] Set up Redis caching layer
- [ ] Configure S3-compatible storage
- [ ] Design sitemap crawler architecture

### Phase 2: News Processing
- [ ] Implement sitemap discovery and parsing
- [ ] Design sitemap hierarchy handler
- [ ] Implement date-based sitemap traversal
- [ ] Create URL pattern analyzer
- [ ] Develop robots.txt parser and handler
- [ ] Implement crawling rules compliance
- [ ] Create sitemap discovery service
- [ ] Develop content scraping with rate limiting
- [ ] Design content chunking strategies
- [ ] Implement hierarchical summarization
- [ ] Design AI provider abstraction layer
- [ ] Implement structured data extraction
- [ ] Create content versioning system
- [ ] Set up change detection pipeline
- [ ] Implement story clustering and grouping
- [ ] Develop cross-source analysis
- [ ] Create source attribution system
- [ ] Implement media asset management
- [ ] Set up content archival system

### Phase 3: Publication
- [ ] Develop social media integrations
- [ ] Create web interface
- [ ] Implement digest management
- [ ] Set up automated posting

### Phase 4: Enhancement
- [ ] Add analytics
- [ ] Implement user feedback
- [ ] Optimize AI processing
- [ ] Add customization options

## 📦 Project Structure
fuddy-duddy/
├── src/
│ ├── Core/
│ │ ├── Collection/
│ │ │ ├── Sitemaps/
│ │ │ │ ├── Parsers/
│ │ │ │ │ ├── Standard/     # Standard XML sitemap parsers
│ │ │ │ │ ├── News/         # Google News sitemap parsers
│ │ │ │ │ ├── Pattern/      # Pattern-based parsers
│ │ │ │ │ └── AI/           # AI-assisted fallback parser
│ │ │ │ ├── Strategies/     # Strategy selection and management
│ │ │ │ ├── Patterns/       # Pattern definitions and matchers
│ │ │ │ └── Cache/          # Parsing strategy cache
│ │ │ ├── RobotsTxt/        # Robots.txt parsing and compliance
│ │ │ │ ├── Parser/         # Robots.txt file parser
│ │ │ │ ├── Rules/          # Crawling rules implementation
│ │ │ │ └── Discovery/      # Sitemap discovery service
│ │ │ ├── Scrapers/          # Content extraction services
│ │ │ └── RateLimiting/      # Rate limiting policies
│ │ ├── Processing/
│ │ │ ├── Extractors/        # Structured data extraction
│ │ │ ├── Versioning/        # Content change tracking
│ │ │ ├── Analysis/          # Content analysis services
│ │ │ ├── Clustering/       # Story grouping and similarity
│ │ │ ├── CrossSource/      # Multi-source analysis
│ │ │ └── Verification/     # Fact checking and validation
│ │ ├── AI/ # AI integration
│ │ └── Infrastructure/ # Shared infrastructure
│ │ ├── Cache/           # Redis caching infrastructure
│ │ ├── Database/         # Database access
│ │ ├── MessageBus/       # RabbitMQ integration
│ │ ├── Search/          # Elasticsearch integration
│ │ └── Storage/         # S3 object storage integration
│ │
│ └── Application/ # Main application
│ ├── API/ # Web API
│ ├── Frontend/ # Web interface
│ └── Services/ # Application services
│
└── tests/ # Test projects

## 🛠️ Setup and Installation

### Local Development Environment

1. Prerequisites
   - Docker and Docker Compose
   - .NET 9 SDK
   - Git

2. Start backing services:
   ```bash
   docker-compose up -d
   ```

3. Service URLs and Credentials:
   - RabbitMQ:
     - AMQP: amqp://fuddy:duddy@localhost:5672
     - Management UI: http://localhost:15672
   - MySQL:
     - Connection: Server=localhost;Database=fuddyduddy;Uid=fuddy;Pwd=duddy
     - Port: 3306
   - Elasticsearch:
     - URL: http://localhost:9200
   - Redis:
     - Connection: localhost:6379
   - MinIO:
     - API: http://localhost:9000
     - Console: http://localhost:9001
     - Access Key: fuddy
     - Secret Key: duddy123

4. Health Check:
   ```bash
   docker-compose ps
   ```

5. Configuration:
   ```bash
   # Development settings
   cp .env.example .env
   
   # Update configuration with your values
   vim .env
   ```

6. Secrets Management:
   - Local: .env file
   - Development: Azure Key Vault
   - Production: AWS Secrets Manager

## 📝 License

MIT

## 🤝 Contributing

[To be added as project matures]

## 🛠️ Development Workflow

### Branch Strategy
- `main` - production ready code
- `develop` - main development branch
- `feature/*` - new features
- `fix/*` - bug fixes
- `release/*` - release preparation

### CI/CD Pipeline
- Build validation
- Unit tests
- Integration tests
- Docker image builds
- Infrastructure as Code deployment

## 🎯 Initial Focus Areas

1. Core Infrastructure Setup
   - Basic project structure
   - Docker environment
   - CI/CD pipeline
   - Logging & monitoring

2. News Collection MVP
   - Single source integration
   - Basic sitemap parsing
   - Simple content extraction
   - Raw storage in S3

3. Basic Processing
   - Simple summarization
   - Initial content structuring
   - Basic digest generation

This allows for:
- Early validation of architecture
- Quick feedback loop
- Incremental complexity addition

