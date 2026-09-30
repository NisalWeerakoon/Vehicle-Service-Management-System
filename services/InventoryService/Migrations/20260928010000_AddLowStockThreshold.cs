using InventoryService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20260928010000_AddLowStockThreshold")]
public partial class AddLowStockThreshold : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "LowStockThreshold",
            table: "SpareParts",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "LowStockThreshold", table: "SpareParts");
    }
}
