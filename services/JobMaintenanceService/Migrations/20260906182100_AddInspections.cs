using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace JobMaintenanceService.Migrations
{
    public partial class AddInspections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    MechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    MechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    InspectionResults = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    IdentifiedProblems = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletionEventId = table.Column<Guid>(type: "char(36)", nullable: true)
                }, constraints: table => { table.PrimaryKey("PK_Inspections", x => x.Id); })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(name: "IX_Inspections_JobCardId", table: "Inspections", column: "JobCardId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Inspections_MechanicId", table: "Inspections", column: "MechanicId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Inspections");
    }
}
