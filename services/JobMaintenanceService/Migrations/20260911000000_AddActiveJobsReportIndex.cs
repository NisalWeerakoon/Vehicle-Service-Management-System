using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMaintenanceService.Migrations;

public partial class AddActiveJobsReportIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_JobCards_Status",
            table: "JobCards",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_JobCards_Status",
            table: "JobCards");
    }
}
