using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Tests.Domain;

public class NewsSummaryTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var title = "Summary Title";
        var article = "This is the summary content of the article.";
        var categoryId = 1;
        
        // Act
        var summary = new NewsSummary(articleId, title, article, categoryId, Language.EN);
        
        // Assert
        Assert.Equal(articleId, summary.NewsArticleId);
        Assert.Equal(title, summary.Title);
        Assert.Equal(article, summary.Article);
        Assert.Equal(categoryId, summary.CategoryId);
        Assert.Equal(Language.EN, summary.Language);
        Assert.Equal(NewsSummaryState.Created, summary.State);
        Assert.NotEqual(Guid.Empty, summary.Id);
        Assert.NotEqual(default, summary.GeneratedAt);
    }
    
    [Fact]
    public void Validate_ShouldChangeStateToValidated()
    {
        // Arrange
        var summary = CreateDefaultSummary();
        
        // Act
        summary.Validate();
        
        // Assert
        Assert.Equal(NewsSummaryState.Validated, summary.State);
    }
    
    [Fact]
    public void Discard_ShouldChangeStateToDiscarded_AndSetReason()
    {
        // Arrange
        var summary = CreateDefaultSummary();
        var reason = "Content is not relevant";
        
        // Act
        summary.Discard(reason);
        
        // Assert
        Assert.Equal(NewsSummaryState.Discarded, summary.State);
        Assert.Equal(reason, summary.Reason);
    }
    
    [Fact]
    public void UpdateCategory_ShouldChangeCategory()
    {
        // Arrange
        var summary = CreateDefaultSummary();
        var newCategoryId = 2;
        
        // Act
        summary.UpdateCategory(newCategoryId);
        
        // Assert
        Assert.Equal(newCategoryId, summary.CategoryId);
    }
    
    [Fact]
    public void Constructor_WithLongTitle_ShouldTruncateTitle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var longTitle = new string('A', 600); // Title longer than allowed
        var article = "This is the summary content of the article.";
        var categoryId = 1;
        
        // Act
        var summary = new NewsSummary(articleId, longTitle, article, categoryId, Language.RU);
        
        // Assert
        Assert.Equal(500, summary.Title.Length); // Based on actual implementation
        Assert.Equal(longTitle.Substring(0, 500), summary.Title);
    }
    
    [Fact]
    public void Constructor_WithLongArticle_ShouldTruncateArticle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var title = "Summary Title";
        var longArticle = new string('A', 3000); // Article longer than allowed
        var categoryId = 1;
        
        // Act
        var summary = new NewsSummary(articleId, title, longArticle, categoryId, Language.RU);
        
        // Assert
        Assert.Equal(2048, summary.Article.Length); // Based on actual implementation
        Assert.Equal(longArticle.Substring(0, 2048), summary.Article);
    }
    
    [Fact]
    public void MarkAsDigested_ShouldChangeStateToDigested()
    {
        // Arrange
        var summary = CreateDefaultSummary();
        
        // Act
        summary.MarkAsDigested();
        
        // Assert
        Assert.Equal(NewsSummaryState.Digested, summary.State);
    }
    
    private NewsSummary CreateDefaultSummary()
    {
        return new NewsSummary(
            Guid.NewGuid(),
            "Test Summary Title",
            "This is a test summary article content.",
            1,
            Language.RU
        );
    }
} 