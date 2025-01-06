using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewsDialectProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SitemapConfig");

            migrationBuilder.DropColumn(
                name: "RobotsTxtCrawlDelay",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "RobotsTxtLastFetched",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "RobotsTxtRules",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "RobotsTxtUrl",
                table: "NewsSources");

            migrationBuilder.AddColumn<string>(
                name: "DialectType",
                table: "NewsSources",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DialectType",
                table: "NewsSources");

            migrationBuilder.AddColumn<int>(
                name: "RobotsTxtCrawlDelay",
                table: "NewsSources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RobotsTxtLastFetched",
                table: "NewsSources",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RobotsTxtRules",
                table: "NewsSources",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RobotsTxtUrl",
                table: "NewsSources",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SitemapConfig",
                columns: table => new
                {
                    NewsSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastSuccessfulFetch = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdateFrequency = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitemapConfig", x => new { x.NewsSourceId, x.Id });
                    table.ForeignKey(
                        name: "FK_SitemapConfig_NewsSources_NewsSourceId",
                        column: x => x.NewsSourceId,
                        principalTable: "NewsSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
