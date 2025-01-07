using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DbFix_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NewsSummaries_NewsArticleId",
                table: "NewsSummaries",
                column: "NewsArticleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
