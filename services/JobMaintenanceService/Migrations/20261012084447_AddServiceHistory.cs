using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace JobMaintenanceService.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    JobCardNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    VehicleRegistrationNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ReportedProblems = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    CompletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    FinalStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedRepairTasksJson = table.Column<string>(type: "longtext", nullable: false),
                    RepairNotesJson = table.Column<string>(type: "longtext", nullable: false),
                    InspectionResults = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    IdentifiedProblems = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    InspectionCompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHistories", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHistories_JobCardId",
                table: "ServiceHistories",
                column: "JobCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHistories_VehicleId_CompletedAt",
                table: "ServiceHistories",
                columns: new[] { "VehicleId", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceHistories");
        }
    }
}
