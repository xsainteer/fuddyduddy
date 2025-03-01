using FuddyDuddy.Core.Application.Configuration;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FuddyDuddy.Core.Tests.Application;

public class SimilarityServiceTests
{
    private readonly Mock<INewsSummaryRepository> _mockSummaryRepository;
    private readonly Mock<ISimilarRepository> _mockSimilarRepository;
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<ILogger<SimilarityService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IOptions<SimilaritySettings>> _mockSimilaritySettings;
    private readonly SimilarityService _service;

    public SimilarityServiceTests()
    {
        _mockSummaryRepository = new Mock<INewsSummaryRepository>();
        _mockSimilarRepository = new Mock<ISimilarRepository>();
        _mockAiService = new Mock<IAiService>();
        _mockLogger = new Mock<ILogger<SimilarityService>>();
        _mockCacheService = new Mock<ICacheService>();
        _mockSimilaritySettings = new Mock<IOptions<SimilaritySettings>>();

        _mockSimilaritySettings.Setup(x => x.Value).Returns(new SimilaritySettings
        {
            Enabled = true,
            MaxSimilarSummaries = 20
        });

        _service = new SimilarityService(
            _mockSummaryRepository.Object,
            _mockSimilarRepository.Object,
            _mockAiService.Object,
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockSimilaritySettings.Object
        );
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenSummaryAlreadyInSimilarityGroup_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var existingSimilarGroups = new List<Similar>
        {
            new Similar("Test Group", Language.EN, new List<SimilarReference>
            {
                new SimilarReference(summaryId, "Test reason")
            })
        };

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSimilarGroups);

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSummaryRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenSummaryNotFound_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsSummary?)null);

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSummaryRepository.Verify(
            x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenNoRecentSummaries_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsSummary>());

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockAiService.Verify(
            x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenAiReturnsNoSimilarSummary_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);
        var recentSummaries = new List<NewsSummary>
        {
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 1", Language.EN),
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 2", Language.EN)
        };

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetGroupedSummariesWithConnectedOnesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<NewsSummary>>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSummaries);

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimilarityResponse { SimilarSummaryId = null, Reason = null });

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSimilarRepository.Verify(
            x => x.AddAsync(It.IsAny<Similar>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenAiReturnsInvalidSimilarSummaryId_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);
        var recentSummaries = new List<NewsSummary>
        {
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 1", Language.EN),
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 2", Language.EN)
        };

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetGroupedSummariesWithConnectedOnesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<NewsSummary>>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSummaries);

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimilarityResponse { SimilarSummaryId = "not-a-guid", Reason = "Test reason" });

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSimilarRepository.Verify(
            x => x.AddAsync(It.IsAny<Similar>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenSimilarSummaryNotFound_ReturnsEarly()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var similarSummaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);
        var recentSummaries = new List<NewsSummary>
        {
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 1", Language.EN),
            CreateTestSummary(Guid.NewGuid(), "Recent Summary 2", Language.EN)
        };

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetGroupedSummariesWithConnectedOnesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<NewsSummary>>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSummaries);

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimilarityResponse { SimilarSummaryId = similarSummaryId.ToString(), Reason = "Test reason" });

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSimilarRepository.Verify(
            x => x.AddAsync(It.IsAny<Similar>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenSimilarSummaryInExistingGroup_AddsReferenceToGroup()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var similarSummaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);
        var similarSummary = CreateTestSummary(similarSummaryId, "Similar Summary", Language.EN);
        var recentSummaries = new List<NewsSummary> { similarSummary };

        var existingGroup = new Similar("Existing Group", Language.EN, new List<SimilarReference>
        {
            new SimilarReference(similarSummaryId, "Existing reason")
        });

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(similarSummaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar> { existingGroup });

        _mockSimilarRepository
            .Setup(x => x.GetGroupedSummariesWithConnectedOnesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<NewsSummary>> 
            {
                { similarSummaryId, new List<NewsSummary> { similarSummary } }
            });

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSummaries);

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimilarityResponse { SimilarSummaryId = similarSummaryId.ToString(), Reason = "Test reason" });

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSimilarRepository.Verify(
            x => x.AddReferenceAsync(
                It.IsAny<Similar>(),
                It.Is<SimilarReference>(r => r.NewsSummaryId == summaryId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockSimilarRepository.Verify(
            x => x.AddAsync(It.IsAny<Similar>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockCacheService.Verify(
            x => x.AddSummaryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task FindSimilarSummariesAsync_WhenSimilarSummaryNotInExistingGroup_CreatesNewGroup()
    {
        // Arrange
        var summaryId = Guid.NewGuid();
        var similarSummaryId = Guid.NewGuid();
        var summary = CreateTestSummary(summaryId, "Test Summary", Language.EN);
        var similarSummary = CreateTestSummary(similarSummaryId, "Similar Summary", Language.EN);
        var recentSummaries = new List<NewsSummary> { similarSummary };

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetBySummaryIdAsync(similarSummaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Similar>());

        _mockSimilarRepository
            .Setup(x => x.GetGroupedSummariesWithConnectedOnesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<NewsSummary>>());

        _mockSummaryRepository
            .Setup(x => x.GetByIdAsync(summaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.IsAny<IList<NewsSummaryState>>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Language?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSummaries);

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<SimilarityResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SimilarityResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimilarityResponse { SimilarSummaryId = similarSummaryId.ToString(), Reason = "Test reason" });

        // Act
        await _service.FindSimilarSummariesAsync(summaryId, CancellationToken.None);

        // Assert
        _mockSimilarRepository.Verify(
            x => x.AddAsync(
                It.Is<Similar>(s => 
                    s.Title == summary.Title && 
                    s.Language == summary.Language && 
                    s.References.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockSimilarRepository.Verify(
            x => x.AddReferenceAsync(It.IsAny<Similar>(), It.IsAny<SimilarReference>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockCacheService.Verify(
            x => x.AddSummaryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private NewsSummary CreateTestSummary(Guid id, string title, Language language)
    {
        var article = new NewsArticle(
            Guid.NewGuid(),
            "https://example.com",
            "Original Title",
            DateTimeOffset.UtcNow);

        var summary = new NewsSummary(
            article.Id,
            title,
            "This is the summary content of the article.",
            1,
            language);

        // Use reflection to set the Id and NewsArticle properties
        var idProperty = typeof(NewsSummary).GetProperty("Id", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (idProperty != null)
        {
            idProperty.SetValue(summary, id);
        }

        var articleProperty = typeof(NewsSummary).GetProperty("NewsArticle", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (articleProperty != null)
        {
            articleProperty.SetValue(summary, article);
        }

        return summary;
    }
} 