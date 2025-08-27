using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellSensorAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class SchedulerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "algorithm",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModified",
                table: "algorithm",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRun",
                table: "algorithm",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduleInterval",
                table: "algorithm",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_algorithm_Enabled",
                table: "algorithm",
                column: "Enabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_algorithm_Enabled",
                table: "algorithm");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "algorithm");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "algorithm");

            migrationBuilder.DropColumn(
                name: "LastRun",
                table: "algorithm");

            migrationBuilder.DropColumn(
                name: "ScheduleInterval",
                table: "algorithm");
        }
    }
}
