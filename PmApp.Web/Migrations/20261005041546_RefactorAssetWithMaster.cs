using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAssetWithMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Line",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Assets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineFunctionId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "McCategoryId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpNo",
                table: "Assets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AreaId",
                table: "Assets",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_BrandId",
                table: "Assets",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LineId",
                table: "Assets",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MachineFunctionId",
                table: "Assets",
                column: "MachineFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_McCategoryId",
                table: "Assets",
                column: "McCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProductId",
                table: "Assets",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Areas_AreaId",
                table: "Assets",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Brands_BrandId",
                table: "Assets",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Lines_LineId",
                table: "Assets",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_MachineFunctions_MachineFunctionId",
                table: "Assets",
                column: "MachineFunctionId",
                principalTable: "MachineFunctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_McCategories_McCategoryId",
                table: "Assets",
                column: "McCategoryId",
                principalTable: "McCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Areas_AreaId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Brands_BrandId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Lines_LineId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_MachineFunctions_MachineFunctionId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_McCategories_McCategoryId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_AreaId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_BrandId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_LineId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_MachineFunctionId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_McCategoryId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_ProductId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LineId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "MachineFunctionId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "McCategoryId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OpNo",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Line",
                table: "Assets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
