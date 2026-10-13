using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddResultToPmSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "PmSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "PmSchedules");
        }
    }
}
