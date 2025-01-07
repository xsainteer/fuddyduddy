using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DbFix_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsSummaries_NewsArticles_NewsArticleId",
                table: "NewsSummaries");

            migrationBuilder.DropIndex(
                name: "IX_NewsSummaries_NewsArticleId",
                table: "NewsSummaries");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsSummaries_NewsArticles_NewsArticleId",
                table: "NewsSummaries",
                column: "NewsArticleId",
                principalTable: "NewsArticles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
