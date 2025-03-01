using FuddyDuddy.Core.Application.Dialects;
using FuddyDuddy.Core.Application.Services;
using System;
using Xunit;

namespace FuddyDuddy.Core.Tests.Application;

public class NewsSourceDialectFactoryTests
{
    private readonly INewsSourceDialectFactory _factory;

    public NewsSourceDialectFactoryTests()
    {
        _factory = new NewsSourceDialectFactory();
    }

    [Theory]
    [InlineData("KNews", typeof(KNewsDialect))]
    [InlineData("Kaktus", typeof(KaktusDialect))]
    [InlineData("Sputnik", typeof(SputnikDialect))]
    [InlineData("AkiPress", typeof(AkiPressDialect))]
    [InlineData("24kg", typeof(TwentyFourKgDialect))]
    [InlineData("Akchabar", typeof(AkchabarDialect))]
    [InlineData("Kloop", typeof(KloopDialect))]
    public void CreateDialect_WithValidDialectType_ReturnsCorrectDialect(string dialectType, Type expectedType)
    {
        // Act
        var dialect = _factory.CreateDialect(dialectType);

        // Assert
        Assert.NotNull(dialect);
        Assert.IsType(expectedType, dialect);
    }

    [Fact]
    public void CreateDialect_WithInvalidDialectType_ThrowsArgumentException()
    {
        // Arrange
        var invalidDialectType = "NonExistentDialect";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _factory.CreateDialect(invalidDialectType));
        Assert.Contains(invalidDialectType, exception.Message);
    }
} 