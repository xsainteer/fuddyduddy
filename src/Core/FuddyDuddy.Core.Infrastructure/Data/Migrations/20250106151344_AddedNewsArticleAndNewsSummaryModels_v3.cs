using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewsArticleAndNewsSummaryModels_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawContent",
                table: "NewsArticles");

            migrationBuilder.AlterColumn<string>(
                name: "Article",
                table: "NewsSummaries",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Article",
                table: "NewsSummaries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RawContent",
                table: "NewsArticles",
                type: "text",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
