using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;
namespace BillingService.Migrations;
[Migration("20260930000000_AddInvoicesAndChargeLines")]
public partial class AddInvoicesAndChargeLines : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"Invoices", columns:t=>new { Id=t.Column<int>(type:"int",nullable:false).Annotation("MySQL:ValueGenerationStrategy",MySQLValueGenerationStrategy.IdentityColumn), JobCardId=t.Column<int>(type:"int",nullable:false), TotalAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false), CreatedAt=t.Column<DateTime>(type:"datetime(6)",nullable:false), UpdatedAt=t.Column<DateTime>(type:"datetime(6)",nullable:false) }, constraints:t=>t.PrimaryKey("PK_Invoices",x=>x.Id));
  m.CreateTable(name:"ChargeLines", columns:t=>new { Id=t.Column<int>(type:"int",nullable:false).Annotation("MySQL:ValueGenerationStrategy",MySQLValueGenerationStrategy.IdentityColumn), InvoiceId=t.Column<int>(type:"int",nullable:false), ChargeType=t.Column<string>(type:"varchar(20)",nullable:false), Description=t.Column<string>(type:"varchar(300)",nullable:false), Quantity=t.Column<decimal>(type:"decimal(18,2)",nullable:false), UnitPrice=t.Column<decimal>(type:"decimal(18,2)",nullable:false), LineTotal=t.Column<decimal>(type:"decimal(18,2)",nullable:false), SourceReferenceId=t.Column<string>(type:"varchar(100)",nullable:true), CreatedAt=t.Column<DateTime>(type:"datetime(6)",nullable:false) }, constraints:t=>{t.PrimaryKey("PK_ChargeLines",x=>x.Id);t.ForeignKey("FK_ChargeLines_Invoices_InvoiceId",x=>x.InvoiceId,"Invoices","Id",onDelete:ReferentialAction.Cascade);});
  m.CreateIndex(name:"IX_Invoices_JobCardId",table:"Invoices",column:"JobCardId",unique:true); m.CreateIndex(name:"IX_ChargeLines_InvoiceId_SourceReferenceId",table:"ChargeLines",columns:new[]{"InvoiceId","SourceReferenceId"},unique:true);
 }
 protected override void Down(MigrationBuilder m) { m.DropTable(name:"ChargeLines");m.DropTable(name:"Invoices"); }
}
