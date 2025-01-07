using System.Net.Http.Json;
using System.Text.Json;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;
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
    private readonly ILogger<NewsProcessingService> _logger;

    private const int DEFAULT_CATEGORY_ID = 16; // "Other" category

    public NewsProcessingService(
        INewsSourceRepository newsSourceRepository,
        INewsArticleRepository newsArticleRepository,
        INewsSummaryRepository newsSummaryRepository,
        ICategoryRepository categoryRepository,
        IHttpClientFactory httpClientFactory,
        NewsSourceDialectFactory dialectFactory,
        ILogger<NewsProcessingService> logger)
    {
        _newsSourceRepository = newsSourceRepository;
        _newsArticleRepository = newsArticleRepository;
        _newsSummaryRepository = newsSummaryRepository;
        _categoryRepository = categoryRepository;
        _httpClientFactory = httpClientFactory;
        _dialectFactory = dialectFactory;
        _logger = logger;
    }

    public async Task ProcessNewsSourcesAsync(CancellationToken cancellationToken = default)
    {
        var activeSources = await _newsSourceRepository.GetActiveSourcesAsync(cancellationToken);
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Id}. {c.Name} ({c.Local})"));
        
        foreach (var source in activeSources)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var dialect = _dialectFactory.CreateDialect(source.DialectType);
                _logger.LogInformation("Processing news source: {Domain} using dialect {Dialect}", 
                    source.Domain, source.DialectType);

                // Get sitemap
                var sitemapContent = await httpClient.GetStringAsync(dialect.SitemapUrl, cancellationToken);
                var newsItems = dialect.ParseSitemap(sitemapContent);

                foreach (var newsItem in newsItems)
                {
                    _logger.LogInformation("News item: {Title} {Url} {PublishedAt}", newsItem.Title, newsItem.Url, newsItem.PublishedAt);
                }
                
                foreach (var newsItem in newsItems)
                {
                    try
                    {
                        if (newsItem.PublishedAt < DateTimeOffset.UtcNow.Date)
                        {
                            _logger.LogInformation("Skipping old news item: {Url}", newsItem.Url);
                            continue;
                        }

                        // Check if already processed
                        if (await _newsArticleRepository.GetByUrlAsync(newsItem.Url, cancellationToken) != null)
                        {
                            _logger.LogInformation("Article already processed: {Url}", newsItem.Url);
                            continue;
                        }

                        // Get article content
                        var newsContent = await httpClient.GetStringAsync(newsItem.Url, cancellationToken);
                        var articleContent = dialect.ExtractArticleContent(newsContent);

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

    private async Task<string> GetOllamaSummaryAsync(string content, string categoryPrompt, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var request = new
        {
            model = "owl/t-lite",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = $@"Ты - умный ассистент, который умеет кратко и четко пересказывать тексты.
                                Выдели заголовок статьи (title). Игнорируй программный код, ссылки, выдели только суть основной статьи в трех предложениях (article).
                                Определи категорию новости (category) из следующего списка:
                                {categoryPrompt}

                                Отвечай строго в JSON формате, для category верни только ID категории (число от 1 до 16)."
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