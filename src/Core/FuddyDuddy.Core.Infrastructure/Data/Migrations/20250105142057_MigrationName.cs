using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NewsSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Domain = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastCrawled = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RobotsTxtUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RobotsTxtCrawlDelay = table.Column<int>(type: "int", nullable: true),
                    RobotsTxtLastFetched = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    RobotsTxtRules = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsSources", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SitemapConfig",
                columns: table => new
                {
                    NewsSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdateFrequency = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    LastSuccessfulFetch = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_NewsSources_Domain",
                table: "NewsSources",
                column: "Domain",
                unique: true);

            SeedInitialNewsSources(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SitemapConfig");

            migrationBuilder.DropTable(
                name: "NewsSources");
        }

        private void SeedInitialNewsSources(MigrationBuilder migrationBuilder)
        {
            // Kaktus Media
            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "Domain", "Name", "IsActive", "LastCrawled", "RobotsTxtUrl", "RobotsTxtCrawlDelay" },
                values: new object[]
                {
                    Guid.NewGuid(),
                    "kaktus.media",
                    "Kaktus Media",
                    true,
                    DateTimeOffset.UtcNow,
                    "https://kaktus.media/robots.txt",
                    null
                });

            // K-News
            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "Domain", "Name", "IsActive", "LastCrawled", "RobotsTxtUrl" },
                values: new object[]
                {
                    Guid.NewGuid(),
                    "knews.kg",
                    "K-News",
                    true,
                    DateTimeOffset.UtcNow,
                    "https://knews.kg/robots.txt"
                });

            // Sputnik Kyrgyzstan
            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "Domain", "Name", "IsActive", "LastCrawled", "RobotsTxtUrl" },
                values: new object[]
                {
                    Guid.NewGuid(),
                    "ru.sputnik.kg",
                    "Sputnik Кыргызстан",
                    true,
                    DateTimeOffset.UtcNow,
                    "https://ru.sputnik.kg/robots.txt"
                });
        }
    }
}
