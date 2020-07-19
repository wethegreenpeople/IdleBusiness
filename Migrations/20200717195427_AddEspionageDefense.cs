using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddEspionageDefense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "EspionageDefenseModifier",
                table: "Purchasables",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "EspionageDefense",
                table: "Business",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EspionageDefenseModifier",
                table: "Purchasables");

            migrationBuilder.DropColumn(
                name: "EspionageDefense",
                table: "Business");
        }
    }
}
