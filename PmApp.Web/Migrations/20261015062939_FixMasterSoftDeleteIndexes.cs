using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixMasterSoftDeleteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_Code",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_SubUnits_Code",
                table: "SubUnits");

            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Plants_Code",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Metodes_Code",
                table: "Metodes");

            migrationBuilder.DropIndex(
                name: "IX_McCategories_Code",
                table: "McCategories");

            migrationBuilder.DropIndex(
                name: "IX_MachineFunctions_Code",
                table: "MachineFunctions");

            migrationBuilder.DropIndex(
                name: "IX_Lines_Code",
                table: "Lines");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Code",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Areas_Code",
                table: "Areas");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Code",
                table: "Units",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_Code",
                table: "SubUnits",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_Code",
                table: "Plants",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Metodes_Code",
                table: "Metodes",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McCategories_Code",
                table: "McCategories",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MachineFunctions_Code",
                table: "MachineFunctions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_Code",
                table: "Lines",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Code",
                table: "Brands",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Code",
                table: "Areas",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_Code",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_SubUnits_Code",
                table: "SubUnits");

            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Plants_Code",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Metodes_Code",
                table: "Metodes");

            migrationBuilder.DropIndex(
                name: "IX_McCategories_Code",
                table: "McCategories");

            migrationBuilder.DropIndex(
                name: "IX_MachineFunctions_Code",
                table: "MachineFunctions");

            migrationBuilder.DropIndex(
                name: "IX_Lines_Code",
                table: "Lines");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Code",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Areas_Code",
                table: "Areas");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Code",
                table: "Units",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_Code",
                table: "SubUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_Code",
                table: "Plants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metodes_Code",
                table: "Metodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McCategories_Code",
                table: "McCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineFunctions_Code",
                table: "MachineFunctions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lines_Code",
                table: "Lines",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Code",
                table: "Brands",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Code",
                table: "Areas",
                column: "Code",
                unique: true);
        }
    }
}
