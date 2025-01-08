# Fuddy-Duddy

A news aggregation and summarization platform that collects news from various sources, processes them with AI, and generates comprehensive digests.

## Overview

Fuddy-Duddy is a private platform that:
- Aggregates news from trusted sources using sitemaps
- Processes news articles using AI for summarization
- Generates daily digests in multiple languages
- Provides a modern web interface for news consumption

## Architecture

The platform consists of:

### Backend (.NET 9)
- Clean Architecture design
- CQRS pattern for data operations
- Async communication with RabbitMQ
- Data storage in MySQL and Elasticsearch
- Object storage with MinIO
- Caching with Redis
- AI-powered processing pipeline

### Frontend (React)
- Modern React with TypeScript
- Tailwind CSS for styling
- Real-time updates
- Responsive design
- Multi-language support

## Infrastructure
- Containerized with Docker
- CI/CD through Azure Pipelines
- Private Docker Hub repository
- Automated deployment
- High availability setup

## Development

This is a private repository. Access is restricted to authorized team members only.

### Prerequisites
- Docker and Docker Compose
- .NET 9 SDK
- Node.js 20+
- Access to private Docker Hub repository

### Local Setup
1. Clone the repository
2. Copy `.env.example` to `.env` and configure
3. Run `docker compose up -d`
4. Access the web interface at `http://localhost:5173`

### Environment Variables
- Database configuration
- API endpoints
- Service credentials
- AI provider settings

## Deployment

Deployment is automated through Azure Pipelines:
- Builds Docker images for API and web
- Pushes to private Docker Hub repository
- Deploys to production server
- Maintains rolling updates

## Private Repository Notice

This repository contains proprietary code and is not intended for public distribution. All rights reserved.

## Design & Branding

### Typography
- The platform uses [Bebas Neue](https://fonts.google.com/specimen/Bebas+Neue) for its logo and headings
- Bebas Neue is loaded from Google Fonts CDN for optimal performance
- The font was chosen for its strong, modern appearance that matches our brand identity

