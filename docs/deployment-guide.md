# Deployment Guide

This guide describes how to deploy Fuddy-Duddy to production environments.

## Prerequisites

- Docker and Docker Compose
- Azure DevOps account (for CI/CD)
- Domain name with Cloudflare DNS
- Production server with minimum requirements:
  - 4 CPU cores
  - 8GB RAM
  - 100GB SSD storage

## Initial Server Setup

### 1. Cloudflare Configuration

1. Add your domain to Cloudflare
2. Configure DNS records:
   ```
   Type  Name             Content              Proxy Status
   A     fuddy-duddy.org  your-server-ip      Proxied
   A     api             your-server-ip      Proxied
   ```
3. Enable SSL/TLS settings:
   - SSL/TLS encryption mode: Full
   - Enable SSL/TLS 1.3
   - Enable Always Use HTTPS
   - Enable Auto Minify for JavaScript, CSS, and HTML (optional)

### 2. Directory Structure

Create the following directory structure on your server:
```bash
/root/docker/
├── fuddyduddy/
│   ├── appsettings.json      # API settings
│   ├── .env                  # Frontend settings
│   ├── mysql/                # MySQL data
│   ├── mysql-backup/        # MySQL backups
│   ├── rabbitmq/            # RabbitMQ data
│   ├── qdrant/
│   │   ├── storage/        # Qdrant storage
│   │   └── snapshots/      # Qdrant snapshots
│   └── qdrant.yaml         # Qdrant config
└── nginx/
    └── default.conf         # Nginx configuration
```

### 3. Configuration Files

#### Nginx Configuration (`nginx/default.conf`)
```nginx
events {
    worker_connections 1024;
}

http {
    resolver 127.0.0.11 valid=30s ipv6=off;
    keepalive_timeout 60;
    keepalive_requests 1024;

    upstream fuddyduddy_backend {
        server fuddyduddy-api:8080;
    }

    upstream fuddyduddy_web {
        server fuddyduddy-web:80;
    }

    # Main API server
    server {
        listen 80;
        server_name api.fuddy-duddy.org;

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN";
        add_header X-XSS-Protection "1; mode=block";
        add_header X-Content-Type-Options "nosniff";
        
        # Trust Cloudflare proxy
        set_real_ip_from 173.245.48.0/20;
        set_real_ip_from 103.21.244.0/22;
        set_real_ip_from 103.22.200.0/22;
        set_real_ip_from 103.31.4.0/22;
        set_real_ip_from 141.101.64.0/18;
        set_real_ip_from 108.162.192.0/18;
        set_real_ip_from 190.93.240.0/20;
        set_real_ip_from 188.114.96.0/20;
        set_real_ip_from 197.234.240.0/22;
        set_real_ip_from 198.41.128.0/17;
        set_real_ip_from 162.158.0.0/15;
        set_real_ip_from 104.16.0.0/13;
        set_real_ip_from 104.24.0.0/14;
        set_real_ip_from 172.64.0.0/13;
        set_real_ip_from 131.0.72.0/22;
        set_real_ip_from 2400:cb00::/32;
        set_real_ip_from 2606:4700::/32;
        set_real_ip_from 2803:f800::/32;
        set_real_ip_from 2405:b500::/32;
        set_real_ip_from 2405:8100::/32;
        set_real_ip_from 2a06:98c0::/29;
        set_real_ip_from 2c0f:f248::/32;

        real_ip_header CF-Connecting-IP;

        location / {
            proxy_pass http://fuddyduddy_backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_next_upstream error timeout invalid_header http_500 http_502 http_503 http_504;
        }
    }

    # Main web server
    server {
        listen 80;
        server_name fuddy-duddy.org;

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN";
        add_header X-XSS-Protection "1; mode=block";
        add_header X-Content-Type-Options "nosniff";
        
        # Trust Cloudflare proxy
        set_real_ip_from 173.245.48.0/20;
        # ... [same Cloudflare IP ranges as above] ...
        real_ip_header CF-Connecting-IP;

        location / {
            proxy_pass http://fuddyduddy_web;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_next_upstream error timeout invalid_header http_500 http_502 http_503 http_504;
        }
    }
}
```

#### Docker Compose (`docker-compose.yml`)
```yaml
services:
  nginx:
    image: nginx:latest
    restart: always
    ports:
      - 80:80    # Only need port 80 since SSL is handled by Cloudflare
    volumes:
      - ./nginx/default.conf:/etc/nginx/nginx.conf:ro

  fuddyduddy-api:
    image: ggsa/fuddyduddy:api-latest
    restart: always
    ports:
      - 8080:80
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - ./fuddyduddy/appsettings.json:/app/appsettings.json:ro

  fuddyduddy-web:
    image: ggsa/fuddyduddy:web-latest
    restart: always
    depends_on:
      - fuddyduddy-api
    ports:
      - 5173:80
    volumes:
      - ./fuddyduddy/.env:/app/.env:ro

  fuddyduddy-qdrant:
    image: qdrant/qdrant:v1.13.1
    volumes:
      - ./fuddyduddy/qdrant/storage:/qdrant/storage
      - ./fuddyduddy/qdrant/snapshots:/qdrant/snapshots
      - ./fuddyduddy/qdrant.yaml:/qdrant/config/config.yaml:ro
    environment:
      - QDRANT__LOG_LEVEL=INFO
      # ... other Qdrant settings ...

  fuddyduddy-redis:
    image: redis:7.4.1
    restart: always
    environment:
      - REDIS_PASSWORD=your_secure_password
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - fuddyduddy_redis_data:/data

  fuddyduddy-mysql:
    image: mysql:8.0.40
    restart: always
    environment:
      - MYSQL_ROOT_PASSWORD=your_secure_password
      - MYSQL_DATABASE=fuddyduddy
      - MYSQL_USER=fuddyduddy
      - MYSQL_PASSWORD=your_secure_password
    volumes:
      - ./fuddyduddy/mysql:/var/lib/mysql

  fuddyduddy-mysql-cron-backup:
    image: fradelg/mysql-cron-backup
    depends_on:
      - fuddyduddy-mysql
    volumes:
      - ./fuddyduddy/mysql-backup:/backup
    environment:
      - MYSQL_HOST=fuddyduddy-mysql
      - MYSQL_USER=root
      - MYSQL_PASS=${MYSQL_ROOT_PASSWORD}
      - MYSQL_DATABASE=fuddyduddy
      - MAX_BACKUPS=15
      - INIT_BACKUP=1
      - CRON_TIME=0 20 * * *
      - GZIP_LEVEL=9
      - MYSQLDUMP_OPTS=--no-tablespaces

  fuddyduddy-rabbitmq:
    image: rabbitmq:4.0.5-management
    environment:
      - RABBITMQ_DEFAULT_USER=fuddy
      - RABBITMQ_DEFAULT_PASS=your_secure_password
    volumes:
      - ./fuddyduddy/rabbitmq:/var/lib/rabbitmq
```

## CI/CD Setup

### Azure DevOps Pipeline

1. Create a new pipeline using the following configuration:

```yaml
trigger:
  branches:
    include:
      - main

variables:
  sshServiceConnection: 'your-ssh-connection'
  containerRegistry: 'dockerhub'
  repository: 'your-dockerhub-repo'
  backendDockerfile: 'src/Application/FuddyDuddy.Api/Dockerfile'
  frontendDockerfile: 'src/Application/fuddyduddy-web-react/Dockerfile'

stages:
- stage: BuildAndPush
  jobs:
  - job: Build
    steps:
    # Login to Docker Hub
    - task: Docker@2
      inputs:
        command: login
        containerRegistry: $(containerRegistry)

    # Build and push backend
    - task: Docker@2
      inputs:
        command: buildAndPush
        repository: $(repository)
        Dockerfile: $(backendDockerfile)
        tags: api-latest

    # Build and push frontend
    - task: Docker@2
      inputs:
        command: buildAndPush
        repository: $(repository)
        Dockerfile: $(frontendDockerfile)
        tags: web-latest

- stage: Deploy
  jobs:
  - deployment: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: SSH@0
            inputs:
              sshEndpoint: $(sshServiceConnection)
              runOptions: inline
              inline: |
                cd /root/docker
                docker pull $(repository):api-latest
                docker pull $(repository):web-latest
                docker compose up -d --no-deps --build fuddyduddy-api fuddyduddy-web
```

2. Configure Azure DevOps:
   - Add Docker Hub service connection
   - Add SSH service connection to your server
   - Set up environment with approval gates if needed

## Backup Strategy

### Database Backups
- Automated daily backups at 20:00 using mysql-cron-backup
- Retains last 15 backups
- Backups are stored in `/root/docker/fuddyduddy/mysql-backup`

### Manual Backup
```bash
cd /root/docker
docker compose exec -T fuddyduddy-mysql mysqldump -u root -p fuddyduddy > backup.sql
```

### Restore from Backup
```bash
cd /root/docker
cat backup.sql | docker compose exec -T fuddyduddy-mysql mysql -u root -p fuddyduddy
```

## Monitoring

### Log Access
```bash
# API logs
docker compose logs -f fuddyduddy-api

# Frontend logs
docker compose logs -f fuddyduddy-web

# Database logs
docker compose logs -f fuddyduddy-mysql
```

### Health Checks
- API: http://api.fuddy-duddy.org/health
- RabbitMQ Management: http://your-server:15672
- MySQL: Check connection pool status
- Redis: Monitor memory usage

## Troubleshooting

### Common Issues

1. Container fails to start
```bash
docker compose logs [service-name]
```

2. Database connection issues
```bash
docker compose exec fuddyduddy-mysql mysql -u root -p
```

3. Cache issues
```bash
docker compose restart fuddyduddy-redis
```

### Rollback Procedure

1. Roll back to previous version:
```bash
docker pull $(repository):api-previous
docker pull $(repository):web-previous
docker tag $(repository):api-previous $(repository):api-latest
docker tag $(repository):web-previous $(repository):web-latest
docker compose up -d --no-deps --build fuddyduddy-api fuddyduddy-web
```

## Security Checklist

- [ ] Set strong passwords in environment files
- [ ] Configure Cloudflare SSL settings
- [ ] Enable Cloudflare security features:
  - [ ] Enable Web Application Firewall (WAF)
  - [ ] Configure rate limiting rules
  - [ ] Enable Bot Fight Mode
  - [ ] Configure Page Rules
- [ ] Set up firewall rules (allow only Cloudflare IPs)
- [ ] Enable automated backups
- [ ] Configure logging
- [ ] Set up monitoring
- [ ] Review access permissions 