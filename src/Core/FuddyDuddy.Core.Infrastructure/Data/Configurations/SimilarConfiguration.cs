using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class SimilarConfiguration : IEntityTypeConfiguration<Similar>
{
    public void Configure(EntityTypeBuilder<Similar> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Language)
            .IsRequired();

        builder.OwnsMany(s => s.References, r =>
        {
            r.WithOwner().HasForeignKey("SimilarId");
            r.HasKey("SimilarId", "NewsSummaryId");

            r.Property(x => x.NewsSummaryId)
                .IsRequired();

            r.Property(x => x.CreatedAt)
                .IsRequired();

            r.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(255);

            r.HasOne(x => x.NewsSummary)
                .WithMany()
                .HasForeignKey(x => x.NewsSummaryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            r.HasIndex(x => x.NewsSummaryId);
        });
    }
}