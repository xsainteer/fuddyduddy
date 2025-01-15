using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class SimilarConfiguration : IEntityTypeConfiguration<Similar>
{
    public void Configure(EntityTypeBuilder<Similar> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasMany(s => s.Summaries)
            .WithOne(s => s.Similar)
            .HasForeignKey(s => s.SimilarId);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.TitleLocal)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsMany(s => s.Summaries, r =>
        {
            r.WithOwner().HasForeignKey("SimilarId");
            r.HasKey("SimilarId", "NewsSummaryId");

            r.Property(x => x.NewsSummaryId)
                .IsRequired();

            r.HasOne(x => x.NewsSummary)
                .WithMany()
                .HasForeignKey(x => x.NewsSummaryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            r.HasIndex(x => x.NewsSummaryId);
        });
    }
}