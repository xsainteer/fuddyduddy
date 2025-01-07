using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class NewsSummaryConfiguration : IEntityTypeConfiguration<NewsSummary>
{
    public void Configure(EntityTypeBuilder<NewsSummary> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Article)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.CategoryId)
            .HasDefaultValue(16)
            .IsRequired(true);

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.NewsArticle)
            .WithOne(x => x.Summary)
            .HasForeignKey<NewsSummary>(x => x.NewsArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.NewsArticleId).IsUnique();
        builder.HasIndex(x => x.GeneratedAt);
    }
} 