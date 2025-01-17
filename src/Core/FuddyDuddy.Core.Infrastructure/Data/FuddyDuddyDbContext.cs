using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data;

public class FuddyDuddyDbContext : DbContext
{
    public DbSet<NewsSource> NewsSources => Set<NewsSource>();
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<NewsSummary> NewsSummaries => Set<NewsSummary>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Digest> Digests => Set<Digest>();
    public DbSet<DigestReference> DigestReferences => Set<DigestReference>();
    public DbSet<Similar> Similars => Set<Similar>();
    public DbSet<SimilarReference> SimilarReferences => Set<SimilarReference>();

    public FuddyDuddyDbContext(DbContextOptions<FuddyDuddyDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuddyDuddyDbContext).Assembly);
    }
} 