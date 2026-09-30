using InventoryService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace InventoryService.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20260929000000_AddKafkaPartIntegration")]
public partial class AddKafkaPartIntegration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "SourceRequestId", table: "PartRequests", type: "int", nullable: true);
        migrationBuilder.AddColumn<string>(name: "JobCardNumber", table: "PartRequests", type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<decimal>(name: "UnitPrice", table: "PartIssues", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>(name: "TotalAmount", table: "PartIssues", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.CreateTable(name: "ProcessedKafkaEvents", columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
            EventId = table.Column<Guid>(type: "char(36)", nullable: false), EventType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false), ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_ProcessedKafkaEvents", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_PartRequests_SourceRequestId", table: "PartRequests", column: "SourceRequestId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ProcessedKafkaEvents_EventId", table: "ProcessedKafkaEvents", column: "EventId", unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProcessedKafkaEvents"); migrationBuilder.DropIndex(name: "IX_PartRequests_SourceRequestId", table: "PartRequests"); migrationBuilder.DropColumn(name: "SourceRequestId", table: "PartRequests"); migrationBuilder.DropColumn(name: "JobCardNumber", table: "PartRequests"); migrationBuilder.DropColumn(name: "UnitPrice", table: "PartIssues"); migrationBuilder.DropColumn(name: "TotalAmount", table: "PartIssues");
    }
}
