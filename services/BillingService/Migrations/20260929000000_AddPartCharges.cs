using BillingService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;
namespace BillingService.Migrations;
[DbContext(typeof(BillingDbContext))]
[Migration("20260929000000_AddPartCharges")]
public partial class AddPartCharges : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) { migrationBuilder.CreateTable(name:"PartCharges",columns:table=>new { Id=table.Column<int>(type:"int",nullable:false).Annotation("MySQL:ValueGenerationStrategy",MySQLValueGenerationStrategy.IdentityColumn), JobCardId=table.Column<int>(type:"int",nullable:false), PartIssueId=table.Column<int>(type:"int",nullable:false), PartRequestId=table.Column<int>(type:"int",nullable:false), SparePartId=table.Column<int>(type:"int",nullable:false), SparePartName=table.Column<string>(type:"varchar(200)",maxLength:200,nullable:false), Quantity=table.Column<int>(type:"int",nullable:false), UnitPrice=table.Column<decimal>(type:"decimal(18,2)",nullable:false), TotalAmount=table.Column<decimal>(type:"decimal(18,2)",nullable:false), CreatedAt=table.Column<DateTime>(type:"datetime(6)",nullable:false)},constraints:table=>table.PrimaryKey("PK_PartCharges",x=>x.Id)); migrationBuilder.CreateTable(name:"ProcessedKafkaEvents",columns:table=>new { Id=table.Column<int>(type:"int",nullable:false).Annotation("MySQL:ValueGenerationStrategy",MySQLValueGenerationStrategy.IdentityColumn), EventId=table.Column<Guid>(type:"char(36)",nullable:false), EventType=table.Column<string>(type:"varchar(100)",maxLength:100,nullable:false), ProcessedAt=table.Column<DateTime>(type:"datetime(6)",nullable:false)},constraints:table=>table.PrimaryKey("PK_ProcessedKafkaEvents",x=>x.Id)); migrationBuilder.CreateIndex(name:"IX_PartCharges_JobCardId",table:"PartCharges",column:"JobCardId"); migrationBuilder.CreateIndex(name:"IX_PartCharges_PartIssueId",table:"PartCharges",column:"PartIssueId",unique:true); migrationBuilder.CreateIndex(name:"IX_ProcessedKafkaEvents_EventId",table:"ProcessedKafkaEvents",column:"EventId",unique:true); }
 protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable(name:"PartCharges"); migrationBuilder.DropTable(name:"ProcessedKafkaEvents"); }
}
