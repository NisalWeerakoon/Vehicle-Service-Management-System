using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace JobMaintenanceService.Migrations;

public partial class AddRepairTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RepairNotes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                JobCardId = table.Column<int>(type: "int", nullable: false),
                MechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                MechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                Note = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_RepairNotes", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RepairTasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                JobCardId = table.Column<int>(type: "int", nullable: false),
                MechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                MechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                TaskTitle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                TaskDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_RepairTasks", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateIndex(name: "IX_RepairNotes_JobCardId", table: "RepairNotes", column: "JobCardId");
        migrationBuilder.CreateIndex(name: "IX_RepairNotes_MechanicId", table: "RepairNotes", column: "MechanicId");
        migrationBuilder.CreateIndex(name: "IX_RepairTasks_JobCardId", table: "RepairTasks", column: "JobCardId");
        migrationBuilder.CreateIndex(name: "IX_RepairTasks_MechanicId", table: "RepairTasks", column: "MechanicId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RepairNotes");
        migrationBuilder.DropTable(name: "RepairTasks");
    }
}
