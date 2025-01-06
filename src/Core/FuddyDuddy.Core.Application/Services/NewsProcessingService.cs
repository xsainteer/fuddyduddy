using System.Net.Http.Json;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Dialects;
using Microsoft.Extensions.Logging;

namespace FuddyDuddy.Core.Application.Services;

public class NewsProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NewsSourceDialectFactory _dialectFactory;
    private readonly ILogger<NewsProcessingService> _logger;

    public NewsProcessingService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        NewsSourceDialectFactory dialectFactory,
        ILogger<NewsProcessingService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _dialectFactory = dialectFactory;
        _logger = logger;
    }

    public async Task ProcessNewsSourcesAsync(CancellationToken cancellationToken = default)
    {
        var activeSources = await _unitOfWork.NewsSourceRepository.GetActiveSourcesAsync(cancellationToken);
        
        foreach (var source in activeSources)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var dialect = _dialectFactory.CreateDialect(source.DialectType);
                _logger.LogInformation("Processing news source: {Domain} using dialect {Dialect}", 
                    source.Domain, source.DialectType);

                // Get sitemap
                var sitemapContent = await httpClient.GetStringAsync(dialect.SitemapUrl, cancellationToken);
                _logger.LogInformation("Sitemap content: {SitemapContent}", sitemapContent);
                var newsItems = dialect.ParseSitemap(sitemapContent);
                var latestNews = newsItems.FirstOrDefault();

                if (latestNews == null)
                {
                    _logger.LogWarning("No news found for {Domain}", source.Domain);
                    continue;
                }

                // Get article content
                var newsContent = await httpClient.GetStringAsync(latestNews.Url, cancellationToken);
                var articleContent = dialect.ExtractArticleContent(newsContent);

                // Process with Ollama
                var summary = await GetOllamaSummaryAsync(articleContent, cancellationToken);
                
                Console.WriteLine($"\nSummary for {source.Domain}:");
                Console.WriteLine($"Title: {latestNews.Title}");
                Console.WriteLine($"Published: {latestNews.PublishedAt}");
                Console.WriteLine($"Summary: {summary}\n");

                // Update last crawled timestamp
                source.UpdateLastCrawled();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing news source {Domain}", source.Domain);
            }
        }
    }

    private async Task<string> GetOllamaSummaryAsync(string content, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var request = new
        {
            model = "owl/t-lite",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "Ты - умный ассистент, который умеет кратко и четко пересказывать тексты. Выдели заголовок статьи (title). Игнорируй программный код, ссылки, выдели только суть основной статьи в трех предложениях (article). Выдели основные три-четыре таги (tags). Отвечай строго в JSON формате."
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
                    tags = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "title", "article", "tags" }
            },
            stream = false,
            options = new
            {
                temperature = 0.3,
                num_ctx = 8192
            }
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:11434/api/chat", request, cancellationToken);
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
} 