using System;
using InventoryService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace InventoryService.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20260928000000_AddPartRequestAndIssue")]
public partial class AddPartRequestAndIssue : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PartRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                JobCardId = table.Column<int>(type: "int", nullable: false),
                SparePartId = table.Column<int>(type: "int", nullable: false),
                RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                RequestingMechanicId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                RequestingMechanicName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IssuedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PartRequests", x => x.Id);
                table.ForeignKey(name: "FK_PartRequests_SpareParts_SparePartId", column: x => x.SparePartId, principalTable: "SpareParts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateTable(
            name: "PartIssues",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                PartRequestId = table.Column<int>(type: "int", nullable: false),
                JobCardId = table.Column<int>(type: "int", nullable: false),
                SparePartId = table.Column<int>(type: "int", nullable: false),
                QuantityIssued = table.Column<int>(type: "int", nullable: false),
                InventoryOfficerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                InventoryOfficerName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                IssuedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PartIssues", x => x.Id);
                table.ForeignKey(name: "FK_PartIssues_PartRequests_PartRequestId", column: x => x.PartRequestId, principalTable: "PartRequests", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_PartIssues_SpareParts_SparePartId", column: x => x.SparePartId, principalTable: "SpareParts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(name: "IX_PartRequests_JobCardId", table: "PartRequests", column: "JobCardId");
        migrationBuilder.CreateIndex(name: "IX_PartRequests_SparePartId", table: "PartRequests", column: "SparePartId");
        migrationBuilder.CreateIndex(name: "IX_PartRequests_Status_RequestedAt", table: "PartRequests", columns: new[] { "Status", "RequestedAt" });
        migrationBuilder.CreateIndex(name: "IX_PartIssues_JobCardId", table: "PartIssues", column: "JobCardId");
        migrationBuilder.CreateIndex(name: "IX_PartIssues_PartRequestId", table: "PartIssues", column: "PartRequestId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PartIssues_SparePartId", table: "PartIssues", column: "SparePartId");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PartIssues");
        migrationBuilder.DropTable(name: "PartRequests");
    }
}
