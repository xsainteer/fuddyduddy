using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PublishedAt)
            .IsRequired();

        builder.Property(x => x.CollectedAt)
            .IsRequired();

        builder.Property(x => x.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.NewsSource)
            .WithMany()
            .HasForeignKey(x => x.NewsSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Url).IsUnique();
        builder.HasIndex(x => x.PublishedAt);
        builder.HasIndex(x => x.IsProcessed);
    }
} 