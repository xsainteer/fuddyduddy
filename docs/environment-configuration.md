# Environment Configuration

This document describes the environment variables and configuration settings required to run Fuddy-Duddy.

## Configuration Files

The project uses the following configuration files:
- `.env` - Environment variables for local development
- `appsettings.json` - Application settings for .NET services
- `docker-compose.yml` - Container configuration

## Required Environment Variables

### Database Configuration
```env
# MySQL
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_DATABASE=fuddyduddy
MYSQL_USER=fuddy
MYSQL_PASSWORD=duddy

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# Qdrant
QDRANT_URL=http://localhost:6333
QDRANT_API_KEY=
```

### Message Queue
```env
# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=fuddy
RABBITMQ_PASSWORD=duddy
RABBITMQ_VHOST=/
```

### Storage
```env
# MinIO
MINIO_ENDPOINT=localhost:9000
MINIO_ACCESS_KEY=fuddy
MINIO_SECRET_KEY=duddy123
MINIO_USE_SSL=false
MINIO_BUCKET_NAME=fuddyduddy
```

### AI Services
```env
# Ollama
OLLAMA_BASE_URL=http://localhost:11434
OLLAMA_MODEL=llama2

# Gemini API
GEMINI_API_KEY=your_api_key_here
```

### Application Settings
```env
# API
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development
JWT_SECRET_KEY=your_secret_key_here
CORS_ORIGINS=http://localhost:5173

# Frontend
VITE_API_URL=http://localhost:5000
```

## Development vs Production

### Development
- Uses local services via Docker Compose
- Debug logging enabled
- CORS allows localhost
- No SSL requirement
- In-memory caching option

### Production
- Uses managed services or production containers
- Minimal logging
- Strict CORS policy
- SSL required
- Distributed caching required

## Security Considerations

### Secrets Management
- Never commit sensitive values to source control
- Use secret management service in production
- Rotate credentials regularly
- Use strong passwords and API keys

### SSL/TLS
- Enable HTTPS in production
- Use valid SSL certificates
- Configure secure headers
- Enable HSTS

## Service Dependencies

### Required Services
- MySQL (8.0+)
- Redis (7.0+)
- RabbitMQ (4.0+)
- Qdrant (1.13+)
- MinIO (latest)

### Optional Services
- Ollama (local LLM)
- Gemini API (cloud LLM)

## Configuration Examples

### Development (`.env.example`)
```env
# Database
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_DATABASE=fuddyduddy
MYSQL_USER=fuddy
MYSQL_PASSWORD=duddy

# Add other sections as needed...
```

### Production (`appsettings.Production.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db;Database=fuddyduddy;User=prod_user;Password=****;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Troubleshooting

### Common Issues
1. Connection refused
   - Check if the service is running
   - Verify port configuration
   - Check firewall settings

2. Authentication failed
   - Verify credentials
   - Check environment variables
   - Ensure service is initialized

3. Service unavailable
   - Check Docker container status
   - Verify resource allocation
   - Check logs for errors

### Health Checks
- API: `http://localhost:5000/health`
- Frontend: `http://localhost:5173/health`
- Services: Individual health check endpoints

## Monitoring

### Logging
- Configure log levels in `appsettings.json`
- Use structured logging
- Set up log aggregation

### Metrics
- Configure metrics collection
- Set up monitoring dashboards
- Configure alerts

## Backup and Recovery

### Database Backups
- Configure automated backups
- Test restore procedures
- Document recovery steps

### Configuration Backups
- Version control configurations
- Document changes
- Maintain backup copies