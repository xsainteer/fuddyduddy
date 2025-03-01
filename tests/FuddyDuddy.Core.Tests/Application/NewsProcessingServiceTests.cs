using FuddyDuddy.Core.Application.Configuration;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application.Dialects;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FuddyDuddy.Core.Tests.Application;

public class NewsProcessingServiceTests
{
    private readonly Mock<INewsSourceRepository> _mockNewsSourceRepository;
    private readonly Mock<INewsArticleRepository> _mockNewsArticleRepository;
    private readonly Mock<INewsSummaryRepository> _mockNewsSummaryRepository;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<INewsSourceDialectFactory> _mockDialectFactory;
    private readonly Mock<ICrawlerMiddleware> _mockCrawlerMiddleware;
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<ILogger<NewsProcessingService>> _mockLogger;
    private readonly Mock<IOptions<ProcessingOptions>> _mockProcessingOptions;
    private readonly INewsProcessingService _service;
    private readonly HttpClient _httpClient;

    public NewsProcessingServiceTests()
    {
        _mockNewsSourceRepository = new Mock<INewsSourceRepository>();
        _mockNewsArticleRepository = new Mock<INewsArticleRepository>();
        _mockNewsSummaryRepository = new Mock<INewsSummaryRepository>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockDialectFactory = new Mock<INewsSourceDialectFactory>();
        _mockCrawlerMiddleware = new Mock<ICrawlerMiddleware>();
        _mockAiService = new Mock<IAiService>();
        _mockLogger = new Mock<ILogger<NewsProcessingService>>();
        _mockProcessingOptions = new Mock<IOptions<ProcessingOptions>>();

        // Setup processing options
        _mockProcessingOptions.Setup(x => x.Value).Returns(new ProcessingOptions
        {
            DefaultCategoryId = 1,
            CountrySpell = "Кыргызстан"
        });

        // Setup HTTP client
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Test content")
            });

        _httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientConstants.CRAWLER))
            .Returns(_httpClient);

        // Setup crawler middleware
        _mockCrawlerMiddleware.Setup(x => x.PrepareRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<string>()))
            .ReturnsAsync((HttpRequestMessage request, string _) => request);

        _service = new NewsProcessingService(
            _mockNewsSourceRepository.Object,
            _mockNewsArticleRepository.Object,
            _mockNewsSummaryRepository.Object,
            _mockHttpClientFactory.Object,
            _mockDialectFactory.Object,
            _mockCrawlerMiddleware.Object,
            _mockAiService.Object,
            _mockLogger.Object,
            _mockProcessingOptions.Object
        );
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WithNoActiveSources_DoesNotProcessAnything()
    {
        // Arrange
        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsSource>());

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockDialectFactory.Verify(x => x.CreateDialect(It.IsAny<string>()), Times.Never);
        _mockHttpClientFactory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WithActiveSources_ProcessesEachSource()
    {
        // Arrange
        var source = new NewsSource("example.com", "Example News", "StandardDialect");
        var sources = new List<NewsSource> { source };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(new List<NewsItem>());

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Set up the crawler middleware
        _mockCrawlerMiddleware.Setup(x => x.PrepareRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<string>()))
            .ReturnsAsync((HttpRequestMessage req, string _) => req);

        // Set up HTTP response
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("sitemap content")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientConstants.CRAWLER))
            .Returns(httpClient);

        // Track if the source was updated
        var sourceWasUpdated = false;
        _mockNewsSourceRepository.Setup(x => x.UpdateAsync(It.IsAny<NewsSource>(), It.IsAny<CancellationToken>()))
            .Callback<NewsSource, CancellationToken>((s, ct) => 
            {
                sourceWasUpdated = true;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsSourceRepository.Verify(x => x.UpdateAsync(It.IsAny<NewsSource>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(sourceWasUpdated, "The source should be updated");
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WhenDialectReturnsNoNewsItems_SkipsProcessing()
    {
        // Arrange
        var sources = new List<NewsSource>
        {
            new NewsSource("example.com", "Example News", "StandardDialect")
        };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(new List<NewsItem>());

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsArticleRepository.Verify(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockNewsSummaryRepository.Verify(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WhenNewsItemIsAlreadyProcessed_SkipsItem()
    {
        // Arrange
        var sources = new List<NewsSource>
        {
            new NewsSource("example.com", "Example News", "StandardDialect")
        };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        var newsItems = new List<NewsItem>
        {
            new NewsItem("https://example.com/news/1", "News 1", DateTimeOffset.UtcNow)
        };

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(newsItems);
        mockDialect.Setup(x => x.ExtractArticleContent(It.IsAny<string>()))
            .Returns("Article content");

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Setup article repository to return an existing article
        _mockNewsArticleRepository.Setup(x => x.GetByUrlAsync(newsItems[0].Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NewsArticle(Guid.NewGuid(), newsItems[0].Url, newsItems[0].Title, newsItems[0].PublishedAt));

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsArticleRepository.Verify(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockNewsSummaryRepository.Verify(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WhenNewsItemIsOld_SkipsItem()
    {
        // Arrange
        var sources = new List<NewsSource>
        {
            new NewsSource("example.com", "Example News", "StandardDialect")
        };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        var newsItems = new List<NewsItem>
        {
            new NewsItem("https://example.com/news/1", "News 1", DateTimeOffset.UtcNow.AddDays(-2))
        };

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(newsItems);

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsArticleRepository.Verify(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockNewsSummaryRepository.Verify(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WhenArticleContentIsEmpty_SkipsItem()
    {
        // Arrange
        var sources = new List<NewsSource>
        {
            new NewsSource("example.com", "Example News", "StandardDialect")
        };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        var newsItems = new List<NewsItem>
        {
            new NewsItem("https://example.com/news/1", "News 1", DateTimeOffset.UtcNow)
        };

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(newsItems);
        mockDialect.Setup(x => x.ExtractArticleContent(It.IsAny<string>()))
            .Returns(string.Empty);

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsArticleRepository.Verify(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockNewsSummaryRepository.Verify(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNewsSourcesAsync_WhenArticleIsValid_ProcessesAndSavesArticleAndSummary()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var source = new NewsSource("example.com", "Example News", "StandardDialect");
        
        // Use reflection to set the Id property
        typeof(NewsSource).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(source, sourceId);
        
        var sources = new List<NewsSource> { source };

        _mockNewsSourceRepository.Setup(x => x.GetActiveSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sources);

        // Create a news item with today's date to ensure it's processed
        var newsItems = new List<NewsItem>
        {
            new NewsItem("https://example.com/news/1", "News 1", DateTimeOffset.UtcNow)
        };

        var mockDialect = new Mock<INewsSourceDialect>();
        mockDialect.Setup(x => x.SitemapUrl).Returns("https://example.com/sitemap.xml");
        mockDialect.Setup(x => x.ParseSitemap(It.IsAny<string>()))
            .Returns(newsItems);
        mockDialect.Setup(x => x.ExtractArticleContent(It.IsAny<string>()))
            .Returns("Article content"); // Make sure this returns non-empty content

        _mockDialectFactory.Setup(x => x.CreateDialect(It.IsAny<string>()))
            .Returns(mockDialect.Object);

        // Setup article repository to return null for GetByUrlAsync (article doesn't exist yet)
        _mockNewsArticleRepository.Setup(x => x.GetByUrlAsync(newsItems[0].Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsArticle?)null);

        // Setup crawler middleware
        _mockCrawlerMiddleware.Setup(x => x.PrepareRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<string>()))
            .ReturnsAsync((HttpRequestMessage req, string _) => req);

        // Setup HTTP client with a handler that returns a successful response
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => 
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (request.RequestUri?.ToString().Contains("sitemap") == true)
                {
                    response.Content = new StringContent("sitemap content");
                }
                else
                {
                    response.Content = new StringContent("article content");
                }
                return response;
            });

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientConstants.CRAWLER))
            .Returns(httpClient);

        // Setup processing options
        _mockProcessingOptions.Setup(x => x.Value).Returns(new ProcessingOptions { DefaultCategoryId = 1 });

        // Setup AI service to return a summary
        _mockAiService.Setup(x => x.GenerateStructuredResponseAsync<SummaryResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SummaryResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummaryResponse
            {
                Title = "Summary Title",
                Article = "Summary content"
            });

        // Capture the article that gets added
        NewsArticle? capturedArticle = null;
        _mockNewsArticleRepository.Setup(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Callback<NewsArticle, CancellationToken>((a, _) => capturedArticle = a)
            .Returns(Task.CompletedTask);

        // Capture the summary that gets added
        NewsSummary? capturedSummary = null;
        _mockNewsSummaryRepository.Setup(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()))
            .Callback<NewsSummary, CancellationToken>((s, _) => capturedSummary = s)
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessNewsSourcesAsync(CancellationToken.None);

        // Assert
        _mockNewsArticleRepository.Verify(x => x.AddAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockNewsSummaryRepository.Verify(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.NotNull(capturedArticle);
        Assert.Equal(sourceId, capturedArticle!.NewsSourceId);
        Assert.Equal(newsItems[0].Url, capturedArticle.Url);
        Assert.Equal(newsItems[0].Title, capturedArticle.Title);
        Assert.Equal(newsItems[0].PublishedAt, capturedArticle.PublishedAt);
        
        Assert.NotNull(capturedSummary);
        Assert.Equal(capturedArticle.Id, capturedSummary!.NewsArticleId);
        Assert.Equal("Summary Title", capturedSummary.Title);
        Assert.Equal("Summary content", capturedSummary.Article);
        Assert.Equal(1, capturedSummary.CategoryId);
    }
} 