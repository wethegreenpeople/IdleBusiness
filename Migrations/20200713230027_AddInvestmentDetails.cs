using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddInvestmentDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "InvestedBusinessCashAtInvestment",
                table: "Investment",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InvestedBusinessCashPerSecondAtInvestment",
                table: "Investment",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "InvestmentType",
                table: "Investment",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvestedBusinessCashAtInvestment",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "InvestedBusinessCashPerSecondAtInvestment",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "InvestmentType",
                table: "Investment");
        }
    }
}
