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

        builder.Property(x => x.Language)
            .IsRequired()
            .HasDefaultValue(Language.RU);

        // Relationships
        builder.HasOne(x => x.NewsArticle)
            .WithMany()
            .HasForeignKey(x => x.NewsArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasOne(x => x.Category)
        //     .WithMany()
        //     .HasForeignKey(x => x.CategoryId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.NewsArticleId);
        builder.HasIndex(x => x.GeneratedAt);
        builder.HasIndex(x => x.Language);
    }
} 