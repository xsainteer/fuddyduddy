using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class NewsSourceConfiguration : IEntityTypeConfiguration<NewsSource>
{
    public void Configure(EntityTypeBuilder<NewsSource> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Domain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.LastCrawled)
            .IsRequired();

        // Configure RobotsTxt as owned entity
        builder.OwnsOne(x => x.RobotsTxt, robotsTxt =>
        {
            robotsTxt.Property(r => r.Url)
                .HasColumnName("RobotsTxtUrl")
                .HasMaxLength(2048);

            robotsTxt.Property(r => r.CrawlDelay)
                .HasColumnName("RobotsTxtCrawlDelay");

            robotsTxt.Property(r => r.LastFetched)
                .HasColumnName("RobotsTxtLastFetched");

            robotsTxt.Property<string?>("RulesJson")
                .HasColumnName("RobotsTxtRules")
                .HasColumnType("json");

            // Ignore the Rules collection as it's handled via JSON
            robotsTxt.Ignore(r => r.Rules);
        });

        // Configure Sitemaps as owned collection
        builder.OwnsMany(x => x.Sitemaps, sitemap =>
        {
            sitemap.Property(s => s.Url)
                .IsRequired()
                .HasMaxLength(2048);

            sitemap.Property(s => s.Type)
                .IsRequired();

            sitemap.Property(s => s.UpdateFrequency)
                .IsRequired();

            sitemap.Property(s => s.LastSuccessfulFetch)
                .IsRequired();
        });

        builder.HasIndex(x => x.Domain)
            .IsUnique();
    }
} 