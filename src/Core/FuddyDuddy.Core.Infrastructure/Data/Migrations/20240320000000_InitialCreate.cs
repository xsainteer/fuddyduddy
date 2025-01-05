using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "NewsSources",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Domain = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                LastCrawled = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                RobotsTxtUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                RobotsTxtCrawlDelay = table.Column<int>(type: "int", nullable: true),
                RobotsTxtLastFetched = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NewsSources", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_NewsSources_Domain",
            table: "NewsSources",
            column: "Domain",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "NewsSources");
    }
} 