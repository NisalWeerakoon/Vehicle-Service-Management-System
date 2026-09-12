using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMaintenanceService.Migrations;

public partial class AddJobStatusHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "JobStatusHistories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", 1),
                JobCardId = table.Column<int>(type: "int", nullable: false),
                FromStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                ToStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                ChangedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                ChangedByRole = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobStatusHistories", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_JobStatusHistories_JobCardId_ChangedAt",
            table: "JobStatusHistories",
            columns: new[] { "JobCardId", "ChangedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "JobStatusHistories");
    }
}
