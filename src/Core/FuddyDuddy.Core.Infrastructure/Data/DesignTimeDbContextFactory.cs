using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FuddyDuddy.Core.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FuddyDuddyDbContext>
{
    public FuddyDuddyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FuddyDuddyDbContext>();
        
        // Use a default connection string for migrations
        const string connectionString = "Server=localhost;Database=fuddyduddy;User=fuddy;Password=duddy;";
        
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            x => x.MigrationsAssembly(typeof(FuddyDuddyDbContext).Assembly.FullName));

        return new FuddyDuddyDbContext(optionsBuilder.Options);
    }
} 