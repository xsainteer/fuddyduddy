using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Models.Broker;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FuddyDuddy.Core.Tests.Application;

public class SummaryTranslationServiceTests
{
    private readonly Mock<INewsSummaryRepository> _mockSummaryRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<SummaryTranslationService>> _mockLogger;
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<IBroker> _mockBroker;
    private readonly ISummaryTranslationService _service;

    public SummaryTranslationServiceTests()
    {
        _mockSummaryRepository = new Mock<INewsSummaryRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<SummaryTranslationService>>();
        _mockAiService = new Mock<IAiService>();
        _mockBroker = new Mock<IBroker>();

        _service = new SummaryTranslationService(
            _mockSummaryRepository.Object,
            _mockCacheService.Object,
            _mockLogger.Object,
            _mockAiService.Object,
            _mockBroker.Object
        );
    }

    [Fact]
    public async Task TranslateSummaryAsync_WhenSummaryNotFound_ReturnsNull()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsSummary?)null);

        // Act
        var result = await _service.TranslateSummaryAsync(summaryId, Language.EN, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TranslateSummaryAsync_WhenTranslationAlreadyExists_ReturnsNull()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var originalSummary = new NewsSummary(
            articleId,
            "Original Title",
            "Original Content",
            1,
            Language.RU);

        var existingTranslation = new NewsSummary(
            articleId,
            "Translated Title",
            "Translated Content",
            1,
            Language.EN);

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalSummary);

        // Mock getting translations by article ID
        _mockSummaryRepository
            .Setup(x => x.GetByNewsArticleIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsSummary> { originalSummary, existingTranslation });

        // Act
        var result = await _service.TranslateSummaryAsync(summaryId, Language.EN, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockAiService.Verify(
            x => x.GenerateStructuredResponseAsync<TranslationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TranslationResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TranslateSummaryAsync_WhenTranslationNeeded_CreatesNewTranslation()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var originalSummary = new NewsSummary(
            articleId,
            "Original Title",
            "Original Content",
            1,
            Language.RU);

        var translationResponse = new TranslationResponse
        {
            Title = "Translated Title",
            Article = "Translated Content"
        };

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalSummary);

        // Mock getting translations by article ID (no existing translation)
        _mockSummaryRepository
            .Setup(x => x.GetByNewsArticleIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsSummary> { originalSummary });

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<TranslationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TranslationResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(translationResponse);

        NewsSummary? capturedSummary = null;
        _mockSummaryRepository
            .Setup(x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()))
            .Callback<NewsSummary, CancellationToken>((s, _) => capturedSummary = s)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.TranslateSummaryAsync(summaryId, Language.EN, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedSummary);
        Assert.Equal(translationResponse.Title, capturedSummary!.Title);
        Assert.Equal(translationResponse.Article, capturedSummary.Article);
        Assert.Equal(Language.EN, capturedSummary.Language);
        Assert.Equal(originalSummary.CategoryId, capturedSummary.CategoryId);
        Assert.Equal(articleId, capturedSummary.NewsArticleId);

        _mockBroker.Verify(
            x => x.PushAsync(
                "similar",
                It.Is<SimilarRequest>(r => r.NewsSummaryId == result!.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TranslatePendingAsync_ProcessesAllValidatedAndDigestedSummaries()
    {
        // Arrange
        var summaries = new List<NewsSummary>
        {
            new NewsSummary(Guid.NewGuid(), "Title 1", "Content 1", 1, Language.RU),
            new NewsSummary(Guid.NewGuid(), "Title 2", "Content 2", 1, Language.RU),
            new NewsSummary(Guid.NewGuid(), "Title 3", "Content 3", 1, Language.RU)
        };

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.Is<IList<NewsSummaryState>>(s => 
                    s.Contains(NewsSummaryState.Validated) && 
                    s.Contains(NewsSummaryState.Digested)),
                It.IsAny<DateTimeOffset>(),
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        // Mock the TranslateSummaryAsync method to return a translated summary for each original summary
        foreach (var summary in summaries)
        {
            var translatedSummary = new NewsSummary(
                summary.NewsArticleId,
                $"Translated {summary.Title}",
                $"Translated {summary.Article}",
                summary.CategoryId,
                Language.EN);

            _mockSummaryRepository
                .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            // Mock getting translations by article ID (no existing translation)
            _mockSummaryRepository
                .Setup(x => x.GetByNewsArticleIdAsync(summary.NewsArticleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NewsSummary> { summary });

            _mockAiService
                .Setup(x => x.GenerateStructuredResponseAsync<TranslationResponse>(
                    It.IsAny<string>(),
                    It.Is<string>(s => s.Contains(summary.Title)),
                    It.IsAny<TranslationResponse>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TranslationResponse
                {
                    Title = $"Translated {summary.Title}",
                    Article = $"Translated {summary.Article}"
                });
        }

        // Act
        await _service.TranslatePendingAsync(Language.EN, CancellationToken.None);

        // Assert
        _mockSummaryRepository.Verify(
            x => x.AddAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()),
            Times.Exactly(summaries.Count));

        _mockBroker.Verify(
            x => x.PushAsync(
                "similar",
                It.IsAny<SimilarRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(summaries.Count));
    }
} 