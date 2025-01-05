using FuddyDuddy.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data;

public class FuddyDuddyDbContext : DbContext
{
    public DbSet<NewsSource> NewsSources => Set<NewsSource>();

    public FuddyDuddyDbContext(DbContextOptions<FuddyDuddyDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuddyDuddyDbContext).Assembly);
    }
} 