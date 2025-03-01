# Development Guidelines

This document outlines the development standards and best practices for the Fuddy-Duddy project.

## Code Style

### C# Guidelines

1. General Principles
   - Write clean, readable, and maintainable code
   - Follow SOLID principles
   - Keep methods small and focused
   - Use meaningful names for variables, methods, and classes

2. Naming Conventions
   - Use PascalCase for class names and public members
   - Use camelCase for private fields and local variables
   - Prefix private fields with underscore (_)
   - Use descriptive names that reveal intent

3. Code Organization
   ```csharp
   namespace FuddyDuddy.Core.Domain
   {
       public class NewsArticle
       {
           private readonly string _url;
           
           public Guid Id { get; private set; }
           
           public NewsArticle(string url)
           {
               _url = url ?? throw new ArgumentNullException(nameof(url));
           }
       }
   }
   ```

4. Error Handling
   - Use exceptions for exceptional cases only
   - Create custom exceptions when needed
   - Always include meaningful error messages
   - Log exceptions appropriately

### Frontend Guidelines

1. React Best Practices
   - Use functional components with hooks
   - Keep components small and focused
   - Use TypeScript for type safety
   - Follow React's performance best practices

2. CSS/Styling
   - Use Tailwind CSS utility classes
   - Follow mobile-first approach
   - Maintain consistent spacing and typography
   - Use CSS modules for component-specific styles

## Project Structure

### Backend Structure
```
src/
├── Core/
│   ├── Domain/          # Entities, Domain Events
│   ├── Application/     # Application Services, Interfaces
│   └── Infrastructure/  # External Service Implementations
├── Application/Api      # API Controllers, Middleware
├── Application/Workers/ # Background Processing Services (this is not implemented yet)
```

### Frontend Structure
```
src/Application/fuddyduddy-web-react/src/
├── components/     # Reusable UI Components
├── pages/         # Route Components
├── services/      # API Client Services
├── hooks/         # Custom React Hooks
└── utils/         # Helper Functions
```

## Testing

### Unit Testing
- Write tests for business logic
- Follow Arrange-Act-Assert pattern
- Use meaningful test names
- Mock external dependencies

Example:
```csharp
[Fact]
public async Task ProcessArticle_WithValidUrl_ExtractsContent()
{
    // Arrange
    var article = new NewsArticle("https://example.com");
    var processor = new ArticleProcessor();

    // Act
    var result = await processor.ProcessAsync(article);

    // Assert
    Assert.NotNull(result.Content);
}
```

### Integration Testing
- Test external service integrations
- Use test containers for dependencies
- Clean up test data
- Use appropriate test categories

## Performance Guidelines

1. Database Access
   - Use async/await for I/O operations
   - Implement proper indexing
   - Use connection pooling
   - Avoid N+1 queries

2. Caching
   - Cache frequently accessed data
   - Use appropriate cache duration
   - Implement cache invalidation
   - Monitor cache hit rates

3. API Design
   - Implement pagination
   - Use appropriate HTTP methods
   - Return appropriate status codes
   - Include proper error responses

## Security Guidelines

1. Input Validation
   - Validate all user input
   - Use parameterized queries
   - Sanitize output
   - Implement rate limiting

2. Authentication/Authorization
   - Use JWT tokens
   - Implement proper role-based access
   - Secure sensitive endpoints
   - Follow least privilege principle

## Logging and Monitoring

1. Logging
   ```csharp
   private readonly ILogger<NewsService> _logger;

   public async Task ProcessArticle(NewsArticle article)
   {
       _logger.LogInformation("Processing article: {ArticleId}", article.Id);
       // Processing logic
   }
   ```

2. Metrics
   - Track important business metrics
   - Monitor system health
   - Set up alerts
   - Use structured logging

## Git Workflow

1. Branch Naming
   ```
   feature/add-article-processing
   bugfix/fix-memory-leak
   hotfix/security-patch
   ```

2. Commit Messages
   ```
   feat: add article processing functionality
   fix: resolve memory leak in worker service
   docs: update API documentation
   ```

3. Pull Requests
   - Keep changes focused
   - Include tests
   - Update documentation
   - Request appropriate reviews

## Deployment

1. Build Process
   - Use multi-stage Docker builds
   - Optimize container images
   - Include health checks
   - Set appropriate resource limits

2. Configuration
   - Use environment variables
   - Secure sensitive data
   - Document all settings
   - Validate configurations

## Documentation

1. Code Documentation
   - Document public APIs
   - Include usage examples
   - Explain complex logic
   - Keep documentation up-to-date

2. API Documentation
   - Document all endpoints
   - Include request/response examples
   - Document error responses
   - Keep OpenAPI spec updated

## Dependencies

1. NuGet Packages
   - Keep dependencies up to date
   - Use stable versions
   - Lock package versions
   - Review security advisories

2. Frontend Packages
   - Use yarn for consistency
   - Keep dependencies minimal
   - Regular security audits
   - Lock package versions 