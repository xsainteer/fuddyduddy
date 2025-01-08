using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _24kgDialect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "Domain", "Name", "IsActive", "LastCrawled", "DialectType" },
                values: new object[] {
                    Guid.NewGuid(),
                    "24.kg",
                    "24kg",
                    true,
                    DateTimeOffset.UtcNow,
                    "24kg"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Domain",
                keyValue: "24.kg");
        }
    }
}
