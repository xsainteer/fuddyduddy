using System.Net.Http.Json;
using System.Text.Json;
using System.IO.Compression;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Services;

public class NewsProcessingService
{
    private readonly INewsSourceRepository _newsSourceRepository;
    private readonly INewsArticleRepository _newsArticleRepository;
    private readonly INewsSummaryRepository _newsSummaryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NewsSourceDialectFactory _dialectFactory;
    private readonly ICrawlerMiddleware _crawlerMiddleware;
    private readonly ILogger<NewsProcessingService> _logger;

    private const int DEFAULT_CATEGORY_ID = 16; // "Other" category

    public NewsProcessingService(
        INewsSourceRepository newsSourceRepository,
        INewsArticleRepository newsArticleRepository,
        INewsSummaryRepository newsSummaryRepository,
        ICategoryRepository categoryRepository,
        IHttpClientFactory httpClientFactory,
        NewsSourceDialectFactory dialectFactory,
        ICrawlerMiddleware crawlerMiddleware,
        ILogger<NewsProcessingService> logger)
    {
        _newsSourceRepository = newsSourceRepository;
        _newsArticleRepository = newsArticleRepository;
        _newsSummaryRepository = newsSummaryRepository;
        _categoryRepository = categoryRepository;
        _httpClientFactory = httpClientFactory;
        _dialectFactory = dialectFactory;
        _crawlerMiddleware = crawlerMiddleware;
        _logger = logger;
    }

    private static async Task<string> ReadResponseContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        // Check content encoding
        var contentEncoding = response.Content.Headers.ContentEncoding;
        
        if (contentEncoding.Contains("gzip"))
        {
            using var gzipStream = new GZipStream(contentStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        else if (contentEncoding.Contains("deflate"))
        {
            using var deflateStream = new DeflateStream(contentStream, CompressionMode.Decompress);
            using var reader = new StreamReader(deflateStream);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        else if (contentEncoding.Contains("br"))
        {
            using var brStream = new BrotliStream(contentStream, CompressionMode.Decompress);
            using var reader = new StreamReader(brStream);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        
        // No compression
        using var defaultReader = new StreamReader(contentStream);
        return await defaultReader.ReadToEndAsync(cancellationToken);
    }

    public async Task ProcessNewsSourcesAsync(CancellationToken cancellationToken = default)
    {
        var activeSources = await _newsSourceRepository.GetActiveSourcesAsync(cancellationToken);
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Id}. {c.Local} ({c.Name})"));
        
        foreach (var source in activeSources)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(Constants.CRAWLER);
                var dialect = _dialectFactory.CreateDialect(source.DialectType);
                _logger.LogInformation("Processing news source: {Domain} using dialect {Dialect}", 
                    source.Domain, source.DialectType);

                // Get sitemap with crawler middleware
                var request = new HttpRequestMessage(HttpMethod.Get, dialect.SitemapUrl);
                request = await _crawlerMiddleware.PrepareRequestAsync(request, source.Domain);
                var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                var sitemapContent = await ReadResponseContentAsync(response, cancellationToken);
                _logger.LogInformation("Sitemap content: {Content}", sitemapContent);
                var newsItems = dialect.ParseSitemap(sitemapContent);

                if (newsItems == null)
                {
                    _logger.LogError("No news items found for {Domain}", source.Domain);
                    continue;
                }

                foreach (var newsItem in newsItems)
                {
                    _logger.LogInformation("News item: {Title} {Url} {PublishedAt}", newsItem.Title, newsItem.Url, newsItem.PublishedAt);
                }

                foreach (var newsItem in newsItems)
                {
                    await ProcessNewsItemAsync(newsItem!, source!, dialect!, categoryPrompt, cancellationToken);
                }

                // Update last crawled timestamp
                source.UpdateLastCrawled();
                await _newsSourceRepository.UpdateAsync(source, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing news source {Domain}", source.Domain);
            }
        }
    }

    private async Task ProcessNewsItemAsync(
        Dialects.NewsItem newsItem,
        NewsSource source,
        Dialects.INewsSourceDialect dialect,
        string categoryPrompt,
        CancellationToken cancellationToken)
    {
        try
        {
            if (newsItem.PublishedAt < DateTimeOffset.UtcNow.Date)
            {
                _logger.LogInformation("Skipping old news item: {Url}", newsItem.Url);
                return;
            }

            // Check if already processed
            if (await _newsArticleRepository.GetByUrlAsync(newsItem.Url, cancellationToken) != null)
            {
                _logger.LogInformation("Article already processed: {Url}", newsItem.Url);
                return;
            }

            // Get article content with crawler middleware
            using var httpClient = _httpClientFactory.CreateClient(Constants.CRAWLER);
            var request = new HttpRequestMessage(HttpMethod.Get, newsItem.Url);
            request = await _crawlerMiddleware.PrepareRequestAsync(request, source.Domain);
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var newsContent = await ReadResponseContentAsync(response, cancellationToken);
            var articleContent = dialect.ExtractArticleContent(newsContent);

            if (string.IsNullOrEmpty(articleContent))
            {
                _logger.LogInformation("Skipping article with empty content: {Url}", newsItem.Url);
                return;
            }

            // Create and save article
            var article = new NewsArticle(
                source.Id,
                newsItem.Url,
                newsItem.Title,
                newsItem.PublishedAt
            );
            await _newsArticleRepository.AddAsync(article, cancellationToken);

            // Process with Ollama
            var summaryJson = await GetOllamaSummaryAsync(articleContent, categoryPrompt, cancellationToken);
            var summary = ParseSummaryResponse(summaryJson);
            
            if (summary != null)
            {
                var newsSummary = new NewsSummary(
                    article.Id,
                    summary.Title,
                    summary.Article,
                    summary.CategoryId
                );
                await _newsSummaryRepository.AddAsync(newsSummary, cancellationToken);
            }

            _logger.LogInformation("Processed article: {Title}", newsItem.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing article {Url}", newsItem.Url);
        }
    }

    private async Task<string> GetOllamaSummaryAsync(string content, string categoryPrompt, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(Constants.OLLAMA);
        var request = new
        {
            model = "owl/t-lite",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = $@"Ты - умный ассистент, который делится своими впечатлениями о прочитанных новостях.
                                Прочитай новостной материал и поделись своим впечатлением о нем.
                                
                                В поле title укажи заголовок статьи.
                                В поле article поделись своим впечатлением о сути новости в трех предложениях, не копируя оригинальный текст.
                                В поле category определи категорию новости из следующего списка:
                                {categoryPrompt}

                                Отвечай строго в JSON формате, для category верни только ID категории (число от 1 до 16).
                                
                                Помни: ты делишься своим впечатлением, а не копируешь или пересказываешь текст. Используй собственные формулировки."
                },
                new
                {
                    role = "user",
                    content = content
                }
            },
            format = new
            {
                type = "object",
                properties = new
                {
                    title = new { type = "string" },
                    article = new { type = "string" },
                    category = new { type = "integer", minimum = 1, maximum = 16 }
                },
                required = new[] { "title", "article", "category" }
            },
            stream = false,
            options = new
            {
                temperature = 0.3,
                num_ctx = 8192
            }
        };

        var response = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        return result?.Message?.Content ?? "No summary generated";
    }

    private class OllamaResponse
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class OllamaSummaryResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("article")]
        public string Article { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public int CategoryId { get; set; } = DEFAULT_CATEGORY_ID;
    }

    private OllamaSummaryResponse? ParseSummaryResponse(string jsonResponse)
    {
        try
        {
            return JsonSerializer.Deserialize<OllamaSummaryResponse>(jsonResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse summary response: {Response}", jsonResponse);
            return null;
        }
    }
} 