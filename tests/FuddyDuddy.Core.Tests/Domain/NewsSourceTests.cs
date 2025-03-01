using FuddyDuddy.Core.Domain.Entities;
using System;
using Xunit;

namespace FuddyDuddy.Core.Tests.Domain;

public class NewsSourceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var domain = "example.com";
        var name = "Example News";
        var dialectType = "StandardDialect";

        // Act
        var newsSource = new NewsSource(domain, name, dialectType);

        // Assert
        Assert.Equal(domain, newsSource.Domain);
        Assert.Equal(name, newsSource.Name);
        Assert.Equal(dialectType, newsSource.DialectType);
        Assert.True(newsSource.IsActive);
        Assert.True(newsSource.LastCrawled <= DateTimeOffset.UtcNow);
        Assert.NotEqual(Guid.Empty, newsSource.Id);
    }

    [Fact]
    public void Constructor_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? domain = null;
        var name = "Example News";
        var dialectType = "StandardDialect";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new NewsSource(domain!, name, dialectType));
        Assert.Equal("domain", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var domain = "example.com";
        string? name = null;
        var dialectType = "StandardDialect";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new NewsSource(domain, name!, dialectType));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDialectType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var domain = "example.com";
        var name = "Example News";
        string? dialectType = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new NewsSource(domain, name, dialectType!));
        Assert.Equal("dialectType", exception.ParamName);
    }

    [Fact]
    public void UpdateLastCrawled_ShouldUpdateLastCrawledTimestamp()
    {
        // Arrange
        var newsSource = new NewsSource("example.com", "Example News", "StandardDialect");
        var originalTimestamp = newsSource.LastCrawled;
        
        // Ensure some time passes
        System.Threading.Thread.Sleep(10);

        // Act
        newsSource.UpdateLastCrawled();

        // Assert
        Assert.True(newsSource.LastCrawled > originalTimestamp);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var newsSource = new NewsSource("example.com", "Example News", "StandardDialect");
        
        // Act
        newsSource.Deactivate();

        // Assert
        Assert.False(newsSource.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var newsSource = new NewsSource("example.com", "Example News", "StandardDialect");
        newsSource.Deactivate(); // First deactivate
        
        // Act
        newsSource.Activate();

        // Assert
        Assert.True(newsSource.IsActive);
    }
} 