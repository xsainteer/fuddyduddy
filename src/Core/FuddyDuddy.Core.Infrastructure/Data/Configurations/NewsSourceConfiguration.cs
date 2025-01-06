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

        builder.Property(x => x.DialectType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.LastCrawled)
            .IsRequired();

        builder.HasIndex(x => x.Domain)
            .IsUnique();
    }
} 