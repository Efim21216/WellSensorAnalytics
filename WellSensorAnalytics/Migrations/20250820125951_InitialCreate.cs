using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WellSensorAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "algorithm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(127)", nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
                    WaterWellId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_algorithm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_result",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Result = table.Column<string>(type: "jsonb", nullable: false),
                    AlgorithmId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_analysis_result_algorithm_AlgorithmId",
                        column: x => x.AlgorithmId,
                        principalTable: "algorithm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_algorithm_Name",
                table: "algorithm",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_result_AlgorithmId",
                table: "analysis_result",
                column: "AlgorithmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_result");

            migrationBuilder.DropTable(
                name: "algorithm");
        }
    }
}
