# FuddyDuddy News Aggregator

A news aggregation and summarization platform that collects news from various sources in Kyrgyzstan and generates AI-powered summaries.

## Features

- 🔄 Automated news collection from configured sources
- 🤖 AI-powered news summarization
- 🏷️ Automatic tagging and categorization
- 💾 Efficient caching with Redis
- 🌓 Dark/Light theme support
- 📱 Responsive design
- ♾️ Infinite scroll news feed
- 🔍 Context-aware news navigation

## Architecture

### Backend Components
- **Core Domain**: Contains business logic and domain entities
- **Infrastructure**: Implements data access and external services
- **Application**: Hosts API endpoints and web interface
- **Clean Architecture** principles with CQRS pattern

### Data Flow
1. **News Collection**
   - Fetch sitemaps from configured sources
   - Parse using source-specific dialects
   - Store raw news content

2. **News Processing**
   - Extract relevant content
   - Generate AI summaries
   - Validate and categorize content
   - Cache processed summaries

3. **Data Access**
   - MySQL for persistent storage
   - Redis for caching and performance
   - Entity Framework Core for ORM

### Frontend (React)
- Modern React with TypeScript
- TanStack Query for data fetching
- Tailwind CSS for styling
- Context-based theme management
- Responsive and accessible components

## Tech Stack

### Backend
- .NET 9
- MySQL (persistent storage)
- Redis (caching)
- Entity Framework Core
- Ollama AI (summarization)

### Frontend
- React 18
- TypeScript
- TanStack Query
- Tailwind CSS
- React Router
- React Hot Toast

## API Endpoints

### News Processing
- `POST /api/newsprocessing/process` - Trigger news collection and processing
- `POST /api/newsprocessing/validate-summaries` - Validate generated summaries
- `POST /api/newsprocessing/rebuild-cache` - Rebuild Redis cache

### Summaries
- `GET /api/summaries` - Get paginated summaries
- `GET /api/summaries/{id}` - Get specific summary
- Query parameters supported:
  - `page`: Page number (default: 0)
  - `pageSize`: Items per page (default: 20)
  - `summaryId`: Get summaries around specific ID

## Development Setup

1. Prerequisites:
   - .NET 9 SDK
   - Node.js 18+
   - MySQL 8+
   - Redis 7+
   - Ollama with owl/t-lite model

2. Backend Setup:
   ```bash
   cd src/Application/FuddyDuddy.Api
   dotnet restore
   dotnet run
   ```

3. Frontend Setup:
   ```bash
   cd src/Application/fuddyduddy-web-react
   npm install
   npm run dev
   ```

## Configuration

Key configuration files:
- `appsettings.json`: Backend configuration
- `.env`: Frontend environment variables
- `package.json`: Frontend dependencies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

MIT License - see LICENSE file for details

