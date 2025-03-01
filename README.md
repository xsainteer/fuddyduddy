# Fuddy-Duddy

A news aggregation and summarization platform that collects news from various sources, processes them with AI, and generates comprehensive digests.

**Website:** [https://fuddy-duddy.org](https://fuddy-duddy.org)

## Overview

Fuddy-Duddy is an open-source platform that:
- Aggregates news from trusted sources using sitemaps
- Processes news articles using AI for summarization
- Generates daily digests in multiple languages
- Provides a modern web interface for news consumption
- Leverages vector search for finding similar articles

## Architecture

The platform consists of:

### Backend (.NET 9)
- Clean Architecture design
- CQRS pattern for data operations
- Async communication with RabbitMQ
- Data storage in MySQL and Elasticsearch
- Vector search with Qdrant
- Object storage with MinIO
- Caching with Redis
- AI-powered processing pipeline using Ollama and Gemini API

### Frontend (React)
- Modern React with TypeScript
- Tailwind CSS for styling
- Real-time updates
- Responsive design
- Multi-language support

## Infrastructure
- Containerized with Docker
- CI/CD through Azure Pipelines
- Automated deployment
- High availability setup

## Roadmap

Here are our planned features and improvements:

### Short-term (1-3 months)
1. Implement proper versioning of builds
   - Adjust pipeline to update build number
   - Propagate version to artifacts
   - Add version information in frontend
2. Dynamic sitemap.xml generation in frontend for new digests
3. Improve search with hybrid search in Qdrant
4. Add more news sources and dialects
5. Implement user authentication and personalization

### Medium-term (3-6 months)
1. Add topic clustering for better digest organization
2. Implement real-time notifications for breaking news
3. Create mobile applications (iOS/Android)
4. Add support for more languages
5. Implement advanced analytics dashboard

### Long-term (6+ months)
1. Develop a recommendation engine based on user preferences
2. Create a plugin system for custom extensions
3. Implement federated learning for improved AI models
4. Add support for multimedia content (videos, podcasts)
5. Develop an API marketplace for third-party integrations

For more detailed information about our plans, including specific tasks and benefits, see our [detailed roadmap](ROADMAP.md).

We welcome contributions in any of these areas! Check our [issues](https://github.com/anurmatov/fuddyduddy/issues) for specific tasks related to these roadmap items.

## Getting Started

### Prerequisites
- Docker and Docker Compose
- .NET 9 SDK
- Node.js 20+

### Local Setup
1. Clone the repository
   ```bash
   git clone https://github.com/anurmatov/fuddyduddy.git
   cd fuddyduddy
   ```

2. Copy `.env.example` to `.env` and configure
   ```bash
   cp .env.example .env
   # Edit .env with your preferred settings
   ```

3. Start the infrastructure services
   ```bash
   docker compose -f docker/docker-compose.yml up -d
   ```

4. Build and run the backend
   ```bash
   cd src
   dotnet build
   dotnet run --project Application/FuddyDuddy.Api
   ```

5. Start the frontend development server
   ```bash
   cd src/Web
   npm install
   npm run dev
   ```

6. Access the web interface at `http://localhost:5173`

### Environment Variables
See [Environment Configuration](docs/environment-configuration.md) for details on required environment variables.

## Documentation

Detailed documentation is available in the `/docs` directory:

- [Architecture Overview](docs/architecture.md)
- [Core Components](docs/core-components.md)
- [Development Guidelines](docs/development-guidelines.md)
- [API Documentation](docs/api-documentation.md)
- [Vector Search Implementation](docs/vector-search-implementation.md)
- [Deployment Guide](docs/deployment-guide.md)

## Contributing

We welcome contributions to Fuddy-Duddy! Please see our [Contributing Guide](CONTRIBUTING.md) for details on:

- Code of Conduct
- Development workflow
- Pull request process
- Coding standards
- Testing requirements

### Community Guidelines

This project adheres to the [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

### Reporting Issues

- For bugs and feature requests, use our [GitHub issue templates](https://github.com/anurmatov/fuddyduddy/issues/new/choose)
- For security vulnerabilities, please review our [Security Policy](SECURITY.md)

## License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0) - see the [LICENSE](LICENSE) file for details.

The AGPL-3.0 license ensures that:
- Anyone can use, modify, and distribute this software
- Any modifications must be made available under the same license
- If you use this software to provide a service over a network, you must make the source code (including any modifications) available to users of that service
- All contributors retain their copyright while granting usage rights under this license

This license was chosen to encourage collaboration while ensuring that improvements to the codebase remain available to the community.

## Acknowledgments

- This project was created as a learning platform for working with large language models and news aggregation
- Special thanks to all contributors and the open-source community

## Design & Branding

### Typography
- The platform uses [Bebas Neue](https://fonts.google.com/specimen/Bebas+Neue) for its logo and headings
- Bebas Neue is loaded from Google Fonts CDN for optimal performance
- The font was chosen for its strong, modern appearance that matches our brand identity

