using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_Parts_PartCodeCategoryId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_SubGrupCategoryId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartCodeCategoryId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SubGrupCategoryId",
                table: "Parts");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Parts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Parts");

            migrationBuilder.AddColumn<int>(
                name: "PartCodeCategoryId",
                table: "Parts",
                type: "int",
                nullable: true);

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
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    PartCodeCategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
    }
}
