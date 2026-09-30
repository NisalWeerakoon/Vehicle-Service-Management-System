using JobMaintenanceService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace JobMaintenanceService.Migrations;

[DbContext(typeof(JobMaintenanceDbContext))]
[Migration("20260929000000_AddPartRequests")]
public partial class AddPartRequests : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "PartRequests", columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
            JobCardId = table.Column<int>(type: "int", nullable: false), SparePartId = table.Column<int>(type: "int", nullable: false), RequestedQuantity = table.Column<int>(type: "int", nullable: false),
            RequestingMechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false), RequestingMechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false), RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_PartRequests", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_PartRequests_JobCardId", table: "PartRequests", column: "JobCardId");
        migrationBuilder.CreateIndex(name: "IX_PartRequests_RequestingMechanicId", table: "PartRequests", column: "RequestingMechanicId");
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "PartRequests");
}
