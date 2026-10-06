using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace NotificationService.Migrations;

[Migration("20261006000000_AddNotifications")]
public partial class AddNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                RecipientUserId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                Type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Notifications", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Notifications_RecipientUserId_CreatedAt", table: "Notifications", columns: new[] { "RecipientUserId", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_Notifications_RecipientUserId_IsRead", table: "Notifications", columns: new[] { "RecipientUserId", "IsRead" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Notifications");
}
