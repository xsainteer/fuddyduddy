using FuddyDuddy.Core.Application.Models.AI;
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

public class MaintenanceServiceTests
{
    private readonly Mock<ISummaryValidationService> _mockValidationService;
    private readonly Mock<INewsSummaryRepository> _mockSummaryRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly IMaintenanceService _service;

    public MaintenanceServiceTests()
    {
        _mockValidationService = new Mock<ISummaryValidationService>();
        _mockSummaryRepository = new Mock<INewsSummaryRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();

        _service = new MaintenanceService(
            _mockValidationService.Object,
            _mockSummaryRepository.Object,
            _mockCategoryRepository.Object
        );
    }

    [Fact]
    public async Task RevisitCategoriesAsync_WithNoSummaries_ReturnsEmptySequence()
    {
        // Arrange
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var categories = new List<Category>
        {
            new Category(1, "Technology", "Technology", "computers, software", "computers, software"),
            new Category(2, "Politics", "Politics", "government, policy", "government, policy")
        };

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.Is<IList<NewsSummaryState>>(s => 
                    s.Contains(NewsSummaryState.Validated) && 
                    s.Contains(NewsSummaryState.Digested)),
                since,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsSummary>());

        _mockCategoryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var results = new List<string>();
        await foreach (var result in _service.RevisitCategoriesAsync(since, CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task RevisitCategoriesAsync_WithSummariesButNoChanges_ReturnsProgressAndNoUpdateMessages()
    {
        // Arrange
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var category1 = new Category(1, "Technology", "Technology", "computers, software", "computers, software");
        var category2 = new Category(2, "Politics", "Politics", "government, policy", "government, policy");
        
        var summary1 = CreateTestSummary(Guid.NewGuid(), "Tech Article", category1);
        var summary2 = CreateTestSummary(Guid.NewGuid(), "Politics Article", category2);
        
        var summaries = new List<NewsSummary> { summary1, summary2 };
        var categories = new List<Category> { category1, category2 };

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.Is<IList<NewsSummaryState>>(s => 
                    s.Contains(NewsSummaryState.Validated) && 
                    s.Contains(NewsSummaryState.Digested)),
                since,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        _mockCategoryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Setup validation responses that don't change categories
        _mockValidationService
            .Setup(x => x.ValidateSummaryAsync(summary1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponse { IsValid = true, Topic = category1.Local });

        _mockValidationService
            .Setup(x => x.ValidateSummaryAsync(summary2, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponse { IsValid = true, Topic = category2.Local });

        // Act
        var results = new List<string>();
        await foreach (var result in _service.RevisitCategoriesAsync(since, CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        Assert.Equal(4, results.Count); // 1 progress + 1 no update message for each summary
        Assert.Contains(results, r => r.StartsWith("PROGRESS:"));
        Assert.Contains(results, r => r.StartsWith("Category NOT updated"));
        
        // Verify no updates were made
        _mockSummaryRepository.Verify(
            x => x.UpdateAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevisitCategoriesAsync_WithCategoryChanges_UpdatesSummaryCategories()
    {
        // Arrange
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var category1 = new Category(1, "Technology", "Technology", "computers, software", "computers, software");
        var category2 = new Category(2, "Politics", "Politics", "government, policy", "government, policy");
        
        var summary1 = CreateTestSummary(Guid.NewGuid(), "Tech Article", category1);
        var summary2 = CreateTestSummary(Guid.NewGuid(), "Politics Article", category2);
        
        var summaries = new List<NewsSummary> { summary1, summary2 };
        var categories = new List<Category> { category1, category2 };

        _mockSummaryRepository
            .Setup(x => x.GetByStateAsync(
                It.Is<IList<NewsSummaryState>>(s => 
                    s.Contains(NewsSummaryState.Validated) && 
                    s.Contains(NewsSummaryState.Digested)),
                since,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        _mockCategoryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Setup validation responses that change categories
        _mockValidationService
            .Setup(x => x.ValidateSummaryAsync(summary1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponse { IsValid = true, Topic = category2.Local }); // Change to Politics

        _mockValidationService
            .Setup(x => x.ValidateSummaryAsync(summary2, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponse { IsValid = true, Topic = category1.Local }); // Change to Technology

        // Act
        var results = new List<string>();
        await foreach (var result in _service.RevisitCategoriesAsync(since, CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        Assert.Equal(4, results.Count); // 1 progress + 1 update message for each summary
        Assert.Contains(results, r => r.StartsWith("PROGRESS:"));
        Assert.Contains(results, r => r.StartsWith("Category updated"));
        
        // Verify updates were made
        _mockSummaryRepository.Verify(
            x => x.UpdateAsync(It.IsAny<NewsSummary>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private NewsSummary CreateTestSummary(Guid articleId, string title, Category category)
    {
        var summary = new NewsSummary(
            articleId,
            title,
            "This is the summary content of the article.",
            category.Id,
            Language.RU);
        
        // Use reflection to set the Category property
        var categoryProperty = typeof(NewsSummary).GetProperty("Category", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (categoryProperty != null)
        {
            categoryProperty.SetValue(summary, category);
        }
            
        return summary;
    }
} 