using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPmScheduleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PmScheduleHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PmScheduleId = table.Column<int>(type: "int", nullable: false),
                    ActionBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusBefore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusAfter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResultBefore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResultAfter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PICBefore = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PICAfter = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlanDateBefore = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanDateAfter = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDateBefore = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDateAfter = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotesBefore = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotesAfter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PmScheduleHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmScheduleHistories_PmSchedules_PmScheduleId",
                        column: x => x.PmScheduleId,
                        principalTable: "PmSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PmScheduleHistories_ActionDate",
                table: "PmScheduleHistories",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PmScheduleHistories_PmScheduleId",
                table: "PmScheduleHistories",
                column: "PmScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PmScheduleHistories");
        }
    }
}
