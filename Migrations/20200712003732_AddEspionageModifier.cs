using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddEspionageModifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "EspionageModifier",
                table: "Purchasables",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "EspionageChance",
                table: "Business",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchasables_PurchasableTypes_PurchasableTypeId",
                table: "Purchasables");

            migrationBuilder.DropTable(
                name: "PurchasableTypes");

            migrationBuilder.DropIndex(
                name: "IX_Purchasables_PurchasableTypeId",
                table: "Purchasables");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Purchasables");

            migrationBuilder.DropColumn(
                name: "EspionageModifier",
                table: "Purchasables");

            migrationBuilder.DropColumn(
                name: "PurchasableTypeId",
                table: "Purchasables");

            migrationBuilder.DropColumn(
                name: "EspionageChance",
                table: "Business");
        }
    }
}
