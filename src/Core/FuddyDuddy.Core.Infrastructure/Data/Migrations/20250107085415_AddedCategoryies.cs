using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedCategoryies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Local = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Local" },
                values: new object[,]
                {
                    { 1, "Politics", "Политика" },
                    { 2, "Economy", "Экономика" },
                    { 3, "Society", "Общество" },
                    { 4, "Culture", "Культура" },
                    { 5, "Sports", "Спорт" },
                    { 6, "Technology", "Технологии" },
                    { 7, "Education", "Образование" },
                    { 8, "Healthcare", "Здравоохранение" },
                    { 9, "Environment", "Экология" },
                    { 10, "Tourism", "Туризм" },
                    { 11, "Agriculture", "Сельское хозяйство" },
                    { 12, "Business", "Бизнес" },
                    { 13, "International", "Международные новости" },
                    { 14, "Crime", "Происшествия" },
                    { 15, "Weather", "Погода" },
                    { 16, "Other", "Другое" }
                });

            
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "NewsSummaries");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "NewsSummaries",
                type: "int",
                nullable: false,
                defaultValue: 16);

            migrationBuilder.CreateIndex(
                name: "IX_NewsSummaries_CategoryId",
                table: "NewsSummaries",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsSummaries_Categories_CategoryId",
                table: "NewsSummaries",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsSummaries_Categories_CategoryId",
                table: "NewsSummaries");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_NewsSummaries_CategoryId",
                table: "NewsSummaries");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "NewsSummaries");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "NewsSummaries",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
