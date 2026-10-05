using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPartCategoriesAndRefactorPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Parts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "Parts",
                newName: "Location");

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxQty",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinQty",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartCodeCategoryId",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Parts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockQty",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubGrupCategoryId",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartCodeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCodeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubGrupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartCodeCategoryId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubGrupCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubGrupCategories_PartCodeCategories_PartCodeCategoryId",
                        column: x => x.PartCodeCategoryId,
                        principalTable: "PartCodeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BrandId",
                table: "Parts",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCodeCategoryId",
                table: "Parts",
                column: "PartCodeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_SubGrupCategoryId",
                table: "Parts",
                column: "SubGrupCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PartCodeCategories_Code",
                table: "PartCodeCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubGrupCategories_Code",
                table: "SubGrupCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubGrupCategories_PartCodeCategoryId",
                table: "SubGrupCategories",
                column: "PartCodeCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Brands_BrandId",
                table: "Parts",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_PartCodeCategories_PartCodeCategoryId",
                table: "Parts",
                column: "PartCodeCategoryId",
                principalTable: "PartCodeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_SubGrupCategories_SubGrupCategoryId",
                table: "Parts",
                column: "SubGrupCategoryId",
                principalTable: "SubGrupCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Brands_BrandId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_PartCodeCategories_PartCodeCategoryId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_SubGrupCategories_SubGrupCategoryId",
                table: "Parts");

            migrationBuilder.DropTable(
                name: "SubGrupCategories");

            migrationBuilder.DropTable(
                name: "PartCodeCategories");

            migrationBuilder.DropIndex(
                name: "IX_Parts_BrandId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartCodeCategoryId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_SubGrupCategoryId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaxQty",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MinQty",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartCodeCategoryId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StockQty",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SubGrupCategoryId",
                table: "Parts");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Parts",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Parts",
                newName: "Brand");
        }
    }
}
