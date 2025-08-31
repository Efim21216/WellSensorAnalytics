using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellSensorAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class NamingAndStartEndResTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analysis_result_algorithm_AlgorithmId",
                table: "analysis_result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_analysis_result",
                table: "analysis_result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_algorithm",
                table: "algorithm");

            migrationBuilder.RenameColumn(
                name: "Result",
                table: "analysis_result",
                newName: "result");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "analysis_result",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "AlgorithmId",
                table: "analysis_result",
                newName: "algorithm_id");

            migrationBuilder.RenameIndex(
                name: "IX_analysis_result_AlgorithmId",
                table: "analysis_result",
                newName: "ix_analysis_result_algorithm_id");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "algorithm",
                newName: "settings");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "algorithm",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "algorithm",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "algorithm",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WaterWellId",
                table: "algorithm",
                newName: "water_well_id");

            migrationBuilder.RenameColumn(
                name: "ScheduleInterval",
                table: "algorithm",
                newName: "schedule_interval");

            migrationBuilder.RenameColumn(
                name: "LastRun",
                table: "algorithm",
                newName: "last_run");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "algorithm",
                newName: "last_modified");

            migrationBuilder.RenameIndex(
                name: "IX_algorithm_Name",
                table: "algorithm",
                newName: "ix_algorithm_name");

            migrationBuilder.RenameIndex(
                name: "IX_algorithm_Enabled",
                table: "algorithm",
                newName: "ix_algorithm_enabled");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "end_timestamp",
                table: "analysis_result",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "start_timestamp",
                table: "analysis_result",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_modified",
                table: "algorithm",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'utc'",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "lookback_interval",
                table: "algorithm",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddPrimaryKey(
                name: "pk_analysis_result",
                table: "analysis_result",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_algorithm",
                table: "algorithm",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_analysis_result_algorithm_algorithm_id",
                table: "analysis_result",
                column: "algorithm_id",
                principalTable: "algorithm",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_analysis_result_algorithm_algorithm_id",
                table: "analysis_result");

            migrationBuilder.DropPrimaryKey(
                name: "pk_analysis_result",
                table: "analysis_result");

            migrationBuilder.DropPrimaryKey(
                name: "pk_algorithm",
                table: "algorithm");

            migrationBuilder.DropColumn(
                name: "end_timestamp",
                table: "analysis_result");

            migrationBuilder.DropColumn(
                name: "start_timestamp",
                table: "analysis_result");

            migrationBuilder.DropColumn(
                name: "lookback_interval",
                table: "algorithm");

            migrationBuilder.RenameColumn(
                name: "result",
                table: "analysis_result",
                newName: "Result");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "analysis_result",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "algorithm_id",
                table: "analysis_result",
                newName: "AlgorithmId");

            migrationBuilder.RenameIndex(
                name: "ix_analysis_result_algorithm_id",
                table: "analysis_result",
                newName: "IX_analysis_result_AlgorithmId");

            migrationBuilder.RenameColumn(
                name: "settings",
                table: "algorithm",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "algorithm",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "algorithm",
                newName: "Enabled");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "algorithm",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "water_well_id",
                table: "algorithm",
                newName: "WaterWellId");

            migrationBuilder.RenameColumn(
                name: "schedule_interval",
                table: "algorithm",
                newName: "ScheduleInterval");

            migrationBuilder.RenameColumn(
                name: "last_run",
                table: "algorithm",
                newName: "LastRun");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "algorithm",
                newName: "LastModified");

            migrationBuilder.RenameIndex(
                name: "ix_algorithm_name",
                table: "algorithm",
                newName: "IX_algorithm_Name");

            migrationBuilder.RenameIndex(
                name: "ix_algorithm_enabled",
                table: "algorithm",
                newName: "IX_algorithm_Enabled");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastModified",
                table: "algorithm",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'utc'");

            migrationBuilder.AddPrimaryKey(
                name: "PK_analysis_result",
                table: "analysis_result",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_algorithm",
                table: "algorithm",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_analysis_result_algorithm_AlgorithmId",
                table: "analysis_result",
                column: "AlgorithmId",
                principalTable: "algorithm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
