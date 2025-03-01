using FuddyDuddy.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace FuddyDuddy.Core.Tests.Domain;

public class SimilarTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var title = "Similar News Group";
        var language = Language.EN;
        var references = new List<SimilarReference>();

        // Act
        var similar = new Similar(title, language, references);

        // Assert
        Assert.Equal(title, similar.Title);
        Assert.Equal(language, similar.Language);
        Assert.Empty(similar.References);
        Assert.NotEqual(Guid.Empty, similar.Id);
    }

    [Fact]
    public void Constructor_WithLongTitle_ShouldTruncateTitle()
    {
        // Arrange
        var longTitle = new string('A', 300); // Title longer than 255 chars
        var language = Language.EN;
        var references = new List<SimilarReference>();

        // Act
        var similar = new Similar(longTitle, language, references);

        // Assert
        Assert.Equal(255, similar.Title.Length);
        Assert.Equal(longTitle.Substring(0, 255), similar.Title);
    }

    [Fact]
    public void Constructor_WithReferences_ShouldSetBackReferences()
    {
        // Arrange
        var title = "Similar News Group";
        var language = Language.EN;
        var references = new List<SimilarReference>
        {
            new SimilarReference(Guid.NewGuid(), "First similar article"),
            new SimilarReference(Guid.NewGuid(), "Second similar article")
        };

        // Act
        var similar = new Similar(title, language, references);

        // Assert
        Assert.Equal(2, similar.References.Count);
        foreach (var reference in similar.References)
        {
            Assert.Equal(similar.Id, reference.SimilarId);
        }
    }

    [Fact]
    public void AddReference_ShouldAddToReferencesCollection()
    {
        // Arrange
        var similar = new Similar("Similar News Group", Language.EN, new List<SimilarReference>());
        var reference = new SimilarReference(Guid.NewGuid(), "New similar article");

        // Act
        similar.AddReference(reference);

        // Assert
        Assert.Single(similar.References);
        Assert.Contains(reference, similar.References);
        Assert.Equal(similar.Id, reference.SimilarId);
    }

    [Fact]
    public void RemoveReference_ShouldRemoveFromReferencesCollection()
    {
        // Arrange
        var reference = new SimilarReference(Guid.NewGuid(), "Article to remove");
        var similar = new Similar(
            "Similar News Group", 
            Language.EN, 
            new List<SimilarReference> { reference });

        // Act
        similar.RemoveReference(reference);

        // Assert
        Assert.Empty(similar.References);
    }
}

public class SimilarReferenceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var reason = "Similar content";

        // Act
        var reference = new SimilarReference(newsSummaryId, reason);

        // Assert
        Assert.Equal(newsSummaryId, reference.NewsSummaryId);
        Assert.Equal(reason, reference.Reason);
        Assert.NotEqual(Guid.Empty, reference.Id);
        Assert.True(reference.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithLongReason_ShouldTruncateReason()
    {
        // Arrange
        var newsSummaryId = Guid.NewGuid();
        var longReason = new string('A', 300); // Reason longer than 255 chars

        // Act
        var reference = new SimilarReference(newsSummaryId, longReason);

        // Assert
        Assert.Equal(255, reference.Reason.Length);
        Assert.Equal(longReason.Substring(0, 255), reference.Reason);
    }
} 