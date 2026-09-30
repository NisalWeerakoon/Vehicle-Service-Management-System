using Microsoft.EntityFrameworkCore.Migrations;

namespace BillingService.Migrations;

[Migration("20261001000000_CompleteInvoiceGeneration")]
public partial class CompleteInvoiceGeneration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "InvoiceNumber", table: "Invoices", type: "varchar(40)", maxLength: 40, nullable: true);
        migrationBuilder.AddColumn<string>(name: "JobCardNumber", table: "Invoices", type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<int>(name: "CustomerId", table: "Invoices", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "VehicleId", table: "Invoices", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>(name: "VehicleRegistrationNumber", table: "Invoices", type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<bool>(name: "IsBillingEligible", table: "Invoices", type: "tinyint(1)", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "IsGenerated", table: "Invoices", type: "tinyint(1)", nullable: false, defaultValue: false);
        migrationBuilder.CreateIndex(name: "IX_Invoices_InvoiceNumber", table: "Invoices", column: "InvoiceNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Invoices_IsBillingEligible_IsGenerated", table: "Invoices", columns: new[] { "IsBillingEligible", "IsGenerated" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Invoices_InvoiceNumber", table: "Invoices");
        migrationBuilder.DropIndex(name: "IX_Invoices_IsBillingEligible_IsGenerated", table: "Invoices");
        migrationBuilder.DropColumn(name: "InvoiceNumber", table: "Invoices");
        migrationBuilder.DropColumn(name: "JobCardNumber", table: "Invoices");
        migrationBuilder.DropColumn(name: "CustomerId", table: "Invoices");
        migrationBuilder.DropColumn(name: "VehicleId", table: "Invoices");
        migrationBuilder.DropColumn(name: "VehicleRegistrationNumber", table: "Invoices");
        migrationBuilder.DropColumn(name: "IsBillingEligible", table: "Invoices");
        migrationBuilder.DropColumn(name: "IsGenerated", table: "Invoices");
    }
}
