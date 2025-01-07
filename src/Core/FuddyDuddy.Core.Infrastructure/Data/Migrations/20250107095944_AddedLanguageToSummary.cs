using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedLanguageToSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "NewsSummaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NewsSummaries_Language",
                table: "NewsSummaries",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_NewsSummaries_Language",
                table: "NewsSummaries");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "NewsSummaries");
        }
    }
}
