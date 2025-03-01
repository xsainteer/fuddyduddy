using FuddyDuddy.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;
using System.Reflection;

namespace FuddyDuddy.Core.Tests.Domain;

public class DigestTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var title = "Daily Digest";
        var content = "This is the content of the digest.";
        var language = Language.EN;
        var periodStart = DateTimeOffset.UtcNow.AddDays(-1);
        var periodEnd = DateTimeOffset.UtcNow;
        var references = new List<DigestReference>();
        var state = DigestState.Created;

        // Act
        var digest = new Digest(title, content, language, periodStart, periodEnd, references, state);

        // Assert
        Assert.Equal(title, digest.Title);
        Assert.Equal(content, digest.Content);
        Assert.Equal(language, digest.Language);
        Assert.Equal(periodStart, digest.PeriodStart);
        Assert.Equal(periodEnd, digest.PeriodEnd);
        Assert.Empty(digest.References);
        Assert.Equal(state, digest.State);
        Assert.NotEqual(Guid.Empty, digest.Id);
        Assert.True(digest.GeneratedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_WithLongTitle_ShouldTruncateTitle()
    {
        // Arrange
        var longTitle = new string('A', 600); // Title longer than 500 chars
        var content = "This is the content of the digest.";
        var language = Language.EN;
        var periodStart = DateTimeOffset.UtcNow.AddDays(-1);
        var periodEnd = DateTimeOffset.UtcNow;
        var references = new List<DigestReference>();
        var state = DigestState.Created;

        // Act
        var digest = new Digest(longTitle, content, language, periodStart, periodEnd, references, state);

        // Assert
        Assert.Equal(500, digest.Title.Length);
        Assert.Equal(longTitle.Substring(0, 500), digest.Title);
    }

    [Fact]
    public void Constructor_WithLongContent_ShouldTruncateContent()
    {
        // Arrange
        var title = "Daily Digest";
        var longContent = new string('A', 5000); // Content longer than 4096 chars
        var language = Language.EN;
        var periodStart = DateTimeOffset.UtcNow.AddDays(-1);
        var periodEnd = DateTimeOffset.UtcNow;
        var references = new List<DigestReference>();
        var state = DigestState.Created;

        // Act
        var digest = new Digest(title, longContent, language, periodStart, periodEnd, references, state);

        // Assert
        Assert.Equal(4096, digest.Content.Length);
        Assert.Equal(longContent.Substring(0, 4096), digest.Content);
    }

    [Fact]
    public void Constructor_WithReferences_ShouldSetBackReferences()
    {
        // Arrange
        var title = "Daily Digest";
        var content = "This is the content of the digest.";
        var language = Language.EN;
        var periodStart = DateTimeOffset.UtcNow.AddDays(-1);
        var periodEnd = DateTimeOffset.UtcNow;
        var references = new List<DigestReference>
        {
            new DigestReference(Guid.NewGuid(), "Article 1", "https://example.com/1", "Important news"),
            new DigestReference(Guid.NewGuid(), "Article 2", "https://example.com/2", "Breaking news")
        };
        var state = DigestState.Created;

        // Act
        var digest = new Digest(title, content, language, periodStart, periodEnd, references, state);

        // Assert
        Assert.Equal(2, digest.References.Count);
        foreach (var reference in digest.References)
        {
            Assert.Equal(digest.Id, reference.DigestId);
        }
    }
}

public class DigestReferenceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var title = "Article Title";
        var url = "https://example.com/article";
        var reason = "Important news";

        // Act
        var reference = new DigestReference(newsSummaryId, title, url, reason);

        // Assert
        Assert.Equal(newsSummaryId, reference.NewsSummaryId);
        Assert.Equal(title, reference.Title);
        Assert.Equal(url, reference.Url);
        Assert.Equal(reason, reference.Reason);
        Assert.NotEqual(Guid.Empty, reference.Id);
    }

    [Fact]
    public void Constructor_WithLongTitle_ShouldTruncateTitle()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var longTitle = new string('A', 600); // Title longer than 500 chars
        var url = "https://example.com/article";
        var reason = "Important news";

        // Act
        var reference = new DigestReference(newsSummaryId, longTitle, url, reason);

        // Assert
        Assert.Equal(500, reference.Title.Length);
        Assert.Equal(longTitle.Substring(0, 500), reference.Title);
    }

    [Fact]
    public void Constructor_WithLongUrl_ShouldTruncateUrl()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var title = "Article Title";
        var longUrl = "https://example.com/" + new string('a', 3000); // URL longer than 2048 chars
        var reason = "Important news";

        // Act
        var reference = new DigestReference(newsSummaryId, title, longUrl, reason);

        // Assert
        Assert.Equal(2048, reference.Url.Length);
        Assert.Equal(longUrl.Substring(0, 2048), reference.Url);
    }

    [Fact]
    public void Constructor_WithLongReason_ShouldTruncateReason()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var title = "Article Title";
        var url = "https://example.com/article";
        var longReason = new string('A', 1200); // Reason longer than 1000 chars

        // Act
        var reference = new DigestReference(newsSummaryId, title, url, longReason);

        // Assert
        Assert.Equal(1000, reference.Reason.Length);
        Assert.Equal(longReason.Substring(0, 1000), reference.Reason);
    }
} 