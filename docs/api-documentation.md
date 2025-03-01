# API Documentation

This document describes the REST API endpoints provided by Fuddy-Duddy.

## Base URL

```
Development: http://localhost:5000
Production: https://api.fuddy-duddy.org
```

## Authentication

The API uses API key authentication for maintenance endpoints only.

```http
api-key: your_secret_key
```

## Search

### Search Summaries

```http
POST /api/search/summaries
Content-Type: application/json

{
  "query": "your search query",
  "limit": 20,
  "language": "RU",
  "categoryIds": [1, 2, 3],
  "sourceIds": ["guid1", "guid2"],
  "fromDate": "2024-01-24T00:00:00Z",
  "toDate": "2024-02-24T23:59:59Z"
}
```

Response:
```json
{
  "items": [
    {
      "summary": {
        "id": "guid",
        "title": "Article Title",
        "content": "Article content...",
        "summary": "AI-generated summary...",
        "publishedAt": "2024-02-24T10:00:00Z",
        "source": {
          "id": "guid",
          "name": "Example News"
        }
      },
      "score": 0.95
    }
  ]
}
```

## Digests

### Get Latest Digests

```http
GET /api/digests
```

Query Parameters:
- `page` (int, default: 0) - Page number
- `pageSize` (int, default: 10) - Items per page
- `language` (string, optional) - Filter by language code (default: RU)

Response:
```json
[
  {
    "id": "guid",
    "date": "2024-02-24",
    "language": "RU",
    "summary": "Daily summary...",
    "articles": [
      {
        "id": "guid",
        "title": "Article Title"
      }
    ]
  }
]
```

### Get Digest by ID

```http
GET /api/digests/{id}
```

Response:
```json
{
  "id": "guid",
  "date": "2024-02-24",
  "language": "RU",
  "summary": "Daily summary...",
  "articles": [
    {
      "id": "guid",
      "title": "Article Title",
      "url": "https://example.com/article",
      "summary": "Article summary..."
    }
  ]
}
```

### Generate Digest

```http
POST /api/digests/generate
```

Query Parameters:
- `language` (string, default: RU) - Language for digest generation

Response:
```json
{
  "message": "Digest generation started"
}
```

## Maintenance Endpoints

All maintenance endpoints require API key authentication.

### Process News

```http
POST /api/maintenance/process-news
api-key: your_secret_key
```

Response:
```json
{
  "message": "News processing started"
}
```

### Validate Summaries

```http
POST /api/maintenance/validate-summaries
api-key: your_secret_key
```

Response:
```json
{
  "message": "Summaries validation started"
}
```

### Rebuild Cache

```http
POST /api/maintenance/rebuild-cache
api-key: your_secret_key
```

Response: Server-Sent Events stream with progress updates

### Rebuild Vector Index

```http
POST /api/maintenance/rebuild-vector-index
api-key: your_secret_key
```

Query Parameters:
- `skipDelete` (boolean, default: false) - Skip deleting existing index

Response:
```json
{
  "message": "Vector index rebuilt"
}
```

### Translate Summary

```http
POST /api/maintenance/translate-summary/{id}
api-key: your_secret_key
```

Query Parameters:
- `targetLanguage` (string) - Target language code

Response: Translated summary object

### Extract Date Range

```http
POST /api/maintenance/extract-date-range
api-key: your_secret_key
Content-Type: application/json

"text to extract date range from"
```

Response: Extracted date range object

## Error Responses

The API uses standard HTTP status codes and returns error details in JSON format:

```json
{
  "message": "Error message description"
}
```

Common Status Codes:
- 200: Success
- 400: Bad Request
- 401: Unauthorized (Invalid API key)
- 404: Not Found
- 500: Internal Server Error

## Pagination

List endpoints support pagination with consistent parameters:
- `page`: Page number (0-based)
- `pageSize`: Items per page

## Caching

The API implements caching for:
- Digests
- Summaries
- Search results

Cache can be rebuilt using the maintenance endpoints.

## News Articles

### Get Latest Articles

```http
GET /api/articles
Authorization: Bearer <token>
```

Query Parameters:
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page
- `language` (string, optional) - Filter by language code
- `category` (string, optional) - Filter by category

Response:
```json
{
  "items": [
    {
      "id": "guid",
      "title": "Article Title",
      "url": "https://example.com/article",
      "publishedAt": "2024-02-24T10:00:00Z",
      "source": {
        "id": "guid",
        "name": "Example News"
      }
    }
  ],
  "totalItems": 100,
  "page": 1,
  "pageSize": 20
}
```

### Get Article Details

```http
GET /api/articles/{id}
Authorization: Bearer <token>
```

Response:
```json
{
  "id": "guid",
  "title": "Article Title",
  "url": "https://example.com/article",
  "content": "Article content...",
  "summary": "AI-generated summary...",
  "publishedAt": "2024-02-24T10:00:00Z",
  "source": {
    "id": "guid",
    "name": "Example News"
  },
  "similar": [
    {
      "id": "guid",
      "title": "Similar Article",
      "similarity": 0.95
    }
  ]
}
```

### Find Similar Articles

```http
GET /api/articles/{id}/similar
Authorization: Bearer <token>
```

Query Parameters:
- `limit` (int, default: 5) - Number of similar articles to return

Response:
```json
{
  "items": [
    {
      "id": "guid",
      "title": "Similar Article",
      "url": "https://example.com/similar",
      "similarity": 0.95
    }
  ]
}
```

## Categories

### List Categories

```http
GET /api/categories
Authorization: Bearer <token>
```

Response:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Technology",
      "slug": "technology"
    }
  ]
}
```

## News Sources

### List Sources

```http
GET /api/sources
Authorization: Bearer <token>
```

Response:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Example News",
      "url": "https://example.com",
      "language": "en"
    }
  ]
}
```

## Rate Limiting

The API implements rate limiting:
- 100 requests per minute per IP
- 1000 requests per hour per authenticated user

Rate limit headers:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1582142640
```

## Versioning

The API version is included in the response headers:
```http
X-API-Version: 1.0.0
```

## CORS

The API supports CORS for specified origins. In development:
```
Access-Control-Allow-Origin: http://localhost:5173
``` 