using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantProductLineHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlantId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Lines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_PlantId",
                table: "Products",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_ProductId",
                table: "Lines",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Products_ProductId",
                table: "Lines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Plants_PlantId",
                table: "Products",
                column: "PlantId",
                principalTable: "Plants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Products_ProductId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Plants_PlantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PlantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Lines_ProductId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "PlantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Lines");
        }
    }
}
