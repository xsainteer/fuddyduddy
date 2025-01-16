using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimilarityModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Similars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Similars", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SimilarReferences",
                columns: table => new
                {
                    SimilarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NewsSummaryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarReferences", x => new { x.SimilarId, x.NewsSummaryId });
                    table.ForeignKey(
                        name: "FK_SimilarReferences_NewsSummaries_NewsSummaryId",
                        column: x => x.NewsSummaryId,
                        principalTable: "NewsSummaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimilarReferences_Similars_SimilarId",
                        column: x => x.SimilarId,
                        principalTable: "Similars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarReferences_NewsSummaryId",
                table: "SimilarReferences",
                column: "NewsSummaryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimilarReferences");

            migrationBuilder.DropTable(
                name: "Similars");
        }
    }
}
