using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AkchabarDialect : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO NewsSources (Id, Domain, Name, IsActive, LastCrawled, DialectType)
                VALUES (UUID(), 'akchabar.kg', 'Akchabar', 1, NOW(), 'Akchabar');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM NewsSources 
                WHERE Domain = 'akchabar.kg';
            ");
        }
    }
}
