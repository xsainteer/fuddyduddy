using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuddyDuddy.Core.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Keywords)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.KeywordsLocal)
            .IsRequired()
            .HasMaxLength(255);
    }
}