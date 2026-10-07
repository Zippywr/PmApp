using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPmTaskSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PmSchedules_AssetParts_AssetPartId",
                table: "PmSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_AssetParts_AssetPartId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_AssetPartId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmSchedules_AssetPartId",
                table: "PmSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PmSchedules_DueDate",
                table: "PmSchedules");

            migrationBuilder.DropColumn(
                name: "PIC",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "Standard",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "SubUnit",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "PmSchedules");

            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "PmSchedules");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "PmTaskTemplates",
                newName: "Remark");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "PmTaskTemplates",
                newName: "WorkHourMinutes");

            migrationBuilder.RenameColumn(
                name: "FrequencyValue",
                table: "PmTaskTemplates",
                newName: "TaskSequence");

            migrationBuilder.RenameColumn(
                name: "FrequencyType",
                table: "PmTaskTemplates",
                newName: "StartMonth");

            migrationBuilder.RenameColumn(
                name: "AssetPartId",
                table: "PmTaskTemplates",
                newName: "PeriodeMonth");

            migrationBuilder.RenameColumn(
                name: "MachineCondition",
                table: "PmSchedules",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "PmSchedules",
                newName: "PlanDate");

            migrationBuilder.RenameColumn(
                name: "AssetPartId",
                table: "PmSchedules",
                newName: "Month");

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdPm",
                table: "PmTaskTemplates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LineSequence",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MachineSequence",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MachineState",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManPower",
                table: "PmTaskTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MethodId",
                table: "PmTaskTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StandardId",
                table: "PmTaskTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubUnitId",
                table: "PmTaskTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "PmTaskTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDate",
                table: "PmSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "PmSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_AssetId",
                table: "PmTaskTemplates",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_IdPm",
                table: "PmTaskTemplates",
                column: "IdPm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_MethodId",
                table: "PmTaskTemplates",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_StandardId",
                table: "PmTaskTemplates",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_SubUnitId",
                table: "PmTaskTemplates",
                column: "SubUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_UnitId",
                table: "PmTaskTemplates",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedules_AssetId",
                table: "PmSchedules",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedules_Year_Month",
                table: "PmSchedules",
                columns: new[] { "Year", "Month" });

            migrationBuilder.AddForeignKey(
                name: "FK_PmSchedules_Assets_AssetId",
                table: "PmSchedules",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_Assets_AssetId",
                table: "PmTaskTemplates",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_Metodes_MethodId",
                table: "PmTaskTemplates",
                column: "MethodId",
                principalTable: "Metodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_Standars_StandardId",
                table: "PmTaskTemplates",
                column: "StandardId",
                principalTable: "Standars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_SubUnits_SubUnitId",
                table: "PmTaskTemplates",
                column: "SubUnitId",
                principalTable: "SubUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_Units_UnitId",
                table: "PmTaskTemplates",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PmSchedules_Assets_AssetId",
                table: "PmSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_Assets_AssetId",
                table: "PmTaskTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_Metodes_MethodId",
                table: "PmTaskTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_Standars_StandardId",
                table: "PmTaskTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_SubUnits_SubUnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_PmTaskTemplates_Units_UnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_AssetId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_IdPm",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_MethodId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_StandardId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_SubUnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmTaskTemplates_UnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PmSchedules_AssetId",
                table: "PmSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PmSchedules_Year_Month",
                table: "PmSchedules");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "IdPm",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "LineSequence",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "MachineSequence",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "MachineState",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "ManPower",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "MethodId",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "StandardId",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "SubUnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "PmTaskTemplates");

            migrationBuilder.DropColumn(
                name: "ActualDate",
                table: "PmSchedules");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "PmSchedules");

            migrationBuilder.RenameColumn(
                name: "WorkHourMinutes",
                table: "PmTaskTemplates",
                newName: "Method");

            migrationBuilder.RenameColumn(
                name: "TaskSequence",
                table: "PmTaskTemplates",
                newName: "FrequencyValue");

            migrationBuilder.RenameColumn(
                name: "StartMonth",
                table: "PmTaskTemplates",
                newName: "FrequencyType");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "PmTaskTemplates",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "PeriodeMonth",
                table: "PmTaskTemplates",
                newName: "AssetPartId");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "PmSchedules",
                newName: "MachineCondition");

            migrationBuilder.RenameColumn(
                name: "PlanDate",
                table: "PmSchedules",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "PmSchedules",
                newName: "AssetPartId");

            migrationBuilder.AddColumn<string>(
                name: "PIC",
                table: "PmTaskTemplates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Standard",
                table: "PmTaskTemplates",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubUnit",
                table: "PmTaskTemplates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "PmTaskTemplates",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "PmSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "PmSchedules",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PmTaskTemplates_AssetPartId",
                table: "PmTaskTemplates",
                column: "AssetPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedules_AssetPartId",
                table: "PmSchedules",
                column: "AssetPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedules_DueDate",
                table: "PmSchedules",
                column: "DueDate");

            migrationBuilder.AddForeignKey(
                name: "FK_PmSchedules_AssetParts_AssetPartId",
                table: "PmSchedules",
                column: "AssetPartId",
                principalTable: "AssetParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PmTaskTemplates_AssetParts_AssetPartId",
                table: "PmTaskTemplates",
                column: "AssetPartId",
                principalTable: "AssetParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
