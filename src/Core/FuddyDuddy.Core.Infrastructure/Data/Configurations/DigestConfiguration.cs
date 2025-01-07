using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class DigestConfiguration : IEntityTypeConfiguration<Digest>
{
    public void Configure(EntityTypeBuilder<Digest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired();

        // Configure DigestReference as owned entity
        builder.OwnsMany(x => x.References, r =>
        {
            r.WithOwner().HasForeignKey("DigestId");
            r.HasKey("DigestId", "NewsSummaryId");
            
            r.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            r.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(2048);

            r.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(1000);

            // Configure relationship with NewsSummary
            r.HasOne(x => x.NewsSummary)
                .WithMany()
                .HasForeignKey(x => x.NewsSummaryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Indexes
        builder.HasIndex(x => x.GeneratedAt);
        builder.HasIndex(x => x.Language);
    }
} 