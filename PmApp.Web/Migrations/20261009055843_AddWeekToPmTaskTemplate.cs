using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekToPmTaskTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Week",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Week",
                table: "PmTaskTemplates");
        }
    }
}
