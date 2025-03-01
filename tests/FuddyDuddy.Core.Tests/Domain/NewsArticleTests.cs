using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Tests.Domain;

public class NewsArticleTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var url = "https://example.com/news/article";
        var title = "Test Article Title";
        var publishedAt = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var article = new NewsArticle(sourceId, url, title, publishedAt);

        // Assert
        Assert.Equal(sourceId, article.NewsSourceId);
        Assert.Equal(url, article.Url);
        Assert.Equal(title, article.Title);
        Assert.Equal(publishedAt, article.PublishedAt);
        Assert.False(article.IsProcessed);
        Assert.NotEqual(Guid.Empty, article.Id);
    }

    [Fact]
    public void Constructor_WithLongTitle_ShouldTruncateTitle()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var url = "https://example.com/news/article";
        var longTitle = new string('A', 600); // Title longer than 500 chars
        var publishedAt = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var article = new NewsArticle(sourceId, url, longTitle, publishedAt);

        // Assert
        Assert.Equal(500, article.Title.Length);
        Assert.Equal(longTitle.Substring(0, 500), article.Title);
    }

    [Fact]
    public void Constructor_WithLongUrl_ShouldTruncateUrl()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var longUrl = "https://example.com/" + new string('a', 3000); // URL longer than 2048 chars
        var title = "Test Article Title";
        var publishedAt = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var article = new NewsArticle(sourceId, longUrl, title, publishedAt);

        // Assert
        Assert.Equal(2048, article.Url.Length);
        Assert.Equal(longUrl.Substring(0, 2048), article.Url);
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetIsProcessedToTrue()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var url = "https://example.com/news/article";
        var title = "Test Article Title";
        var publishedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var article = new NewsArticle(sourceId, url, title, publishedAt);
        
        // Act
        article.MarkAsProcessed();
        
        // Assert
        Assert.True(article.IsProcessed);
    }
} 