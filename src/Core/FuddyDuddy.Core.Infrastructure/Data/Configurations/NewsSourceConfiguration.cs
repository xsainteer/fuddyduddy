using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.ValueObjects;
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

        builder.OwnsOne(x => x.RobotsTxt, robotsTxt =>
        {
            robotsTxt.Property(r => r.Url)
                .HasColumnName("RobotsTxtUrl")
                .HasMaxLength(2048);

            robotsTxt.Property(r => r.CrawlDelay)
                .HasColumnName("RobotsTxtCrawlDelay");

            robotsTxt.Property(r => r.LastFetched)
                .HasColumnName("RobotsTxtLastFetched");
        });

        builder.HasIndex(x => x.Domain)
            .IsUnique();
    }
} 