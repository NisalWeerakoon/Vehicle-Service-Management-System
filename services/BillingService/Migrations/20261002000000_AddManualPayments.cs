using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace BillingService.Migrations;

[Migration("20261002000000_AddManualPayments")]
public partial class AddManualPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(name: "AmountPaid", table: "Invoices", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<string>(name: "PaymentStatus", table: "Invoices", type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid");
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                InvoiceId = table.Column<int>(type: "int", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ReferenceNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey("FK_Payments_Invoices_InvoiceId", x => x.InvoiceId, "Invoices", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(name: "IX_Payments_InvoiceId", table: "Payments", column: "InvoiceId");
        migrationBuilder.CreateIndex(name: "IX_Payments_ReferenceNumber", table: "Payments", column: "ReferenceNumber", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Payments");
        migrationBuilder.DropColumn(name: "AmountPaid", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaymentStatus", table: "Invoices");
    }
}
