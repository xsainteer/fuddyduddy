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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FuddyDuddy.Core.Tests.Application;

public class SummaryValidationServiceTests
{
    private readonly Mock<INewsSummaryRepository> _mockSummaryRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<SummaryValidationService>> _mockLogger;
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<IBroker> _mockBroker;
    private readonly ISummaryValidationService _service;

    public SummaryValidationServiceTests()
    {
        _mockSummaryRepository = new Mock<INewsSummaryRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<SummaryValidationService>>();
        _mockAiService = new Mock<IAiService>();
        _mockBroker = new Mock<IBroker>();

        // Create the actual SummaryValidationService
        _service = new SummaryValidationService(
            _mockSummaryRepository.Object,
            _mockCategoryRepository.Object,
            _mockCacheService.Object,
            _mockLogger.Object,
            _mockAiService.Object,
            _mockBroker.Object
        );
    }

    [Fact]
    public async Task ValidateSummaryAsync_WhenAiServiceReturnsValidResponse_ReturnsValidationResponse()
    {
        // Arrange
        var summary = CreateTestSummary();
        var categoryPrompt = "Technology (computers, software, hardware)";
        var expectedResponse = new ValidationResponse
        {
            IsValid = true,
            Reason = "The summary accurately reflects the original article",
            Topic = "Technology"
        };

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<ValidationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ValidationResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ValidateSummaryAsync(summary, categoryPrompt, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse.IsValid, result.IsValid);
        Assert.Equal(expectedResponse.Reason, result.Reason);
        Assert.Equal(expectedResponse.Topic, result.Topic);
    }

    [Fact]
    public async Task ValidateSummaryAsync_WhenAiServiceReturnsNull_ReturnsInvalidResponse()
    {
        // Arrange
        var summary = CreateTestSummary();
        var categoryPrompt = "Technology (computers, software, hardware)";

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<ValidationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ValidationResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValidationResponse?)null);

        // Act
        var result = await _service.ValidateSummaryAsync(summary, categoryPrompt, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Failed to get validation response", result.Reason);
    }

    [Fact]
    public async Task ValidateSummaryAsync_WhenExceptionOccurs_ReturnsInvalidResponseWithErrorMessage()
    {
        // Arrange
        var summary = CreateTestSummary();
        var categoryPrompt = "Technology (computers, software, hardware)";
        var exceptionMessage = "Test exception";

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<ValidationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ValidationResponse>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.ValidateSummaryAsync(summary, categoryPrompt, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(exceptionMessage, result.Reason);
    }

    [Fact]
    public async Task ValidateNewSummariesAsync_ProcessesAllNewSummaries()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category(1, "Technology", "Technology", "computers, software", "computers, software"),
            new Category(2, "Politics", "Politics", "government, policy", "government, policy")
        };

        // Create summaries with proper Category property
        var summaries = new List<NewsSummary>
        {
            CreateTestSummary(categories[0]),
            CreateTestSummary(categories[0]),
            CreateTestSummary(categories[0])
        };

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.Is<IList<NewsSummaryState>>(s => s.Contains(NewsSummaryState.Created)), 
                null, null, null, null, null, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        _mockCategoryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var validationResponse = new ValidationResponse
        {
            IsValid = true,
            Reason = "Valid summary",
            Topic = "Technology"
        };

        _mockAiService
            .Setup(x => x.GenerateStructuredResponseAsync<ValidationResponse>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ValidationResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResponse);

        // Act
        await _service.ValidateNewSummariesAsync(CancellationToken.None);

        // Assert
        _mockSummaryRepository.Verify(
            x => x.UpdateAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce());
    }

    private NewsSummary CreateTestSummary(Category? category = null)
    {
        var articleId = Guid.NewGuid();
        var newsArticle = new NewsArticle(
            Guid.NewGuid(),
            "https://example.com/article",
            "Original Article Title",
            DateTimeOffset.UtcNow.AddHours(-1)
        );
        
        var summary = new NewsSummary(
            articleId,
            "Summary Title",
            "This is the summary content of the article.",
            category?.Id ?? 1,
            Language.RU
        );
        
        // Use reflection to set the NewsArticle property since it's private
        var articleProperty = typeof(NewsSummary).GetProperty("NewsArticle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (articleProperty != null)
        {
            articleProperty.SetValue(summary, newsArticle);
        }
        
        // Use reflection to set the Category property if provided
        if (category != null)
        {
            var categoryProperty = typeof(NewsSummary).GetProperty("Category", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (categoryProperty != null)
            {
                categoryProperty.SetValue(summary, category);
            }
        }
            
        return summary;
    }
} 