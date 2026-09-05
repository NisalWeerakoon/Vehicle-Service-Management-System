using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace JobMaintenanceService.Migrations
{
    /// <inheritdoc />
    public partial class AddMechanicAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JobCardNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    CheckInId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    VehicleRegistrationNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    ReportedProblems = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCards", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MechanicAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    MechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    MechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    AssignedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MechanicAssignments", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProcessedKafkaEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false),
                    EventType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedKafkaEvents", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JobCards_CheckInId",
                table: "JobCards",
                column: "CheckInId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobCards_JobCardNumber",
                table: "JobCards",
                column: "JobCardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MechanicAssignments_JobCardId_IsActive",
                table: "MechanicAssignments",
                columns: new[] { "JobCardId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedKafkaEvents_EventId",
                table: "ProcessedKafkaEvents",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobCards");

            migrationBuilder.DropTable(
                name: "MechanicAssignments");

            migrationBuilder.DropTable(
                name: "ProcessedKafkaEvents");
        }
    }
}
