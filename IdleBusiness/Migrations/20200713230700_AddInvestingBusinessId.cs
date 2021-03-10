using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddInvestingBusinessId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Business_BusinessId",
                table: "Investment");

            migrationBuilder.DropIndex(
                name: "IX_Investment_BusinessId",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Investment");

            migrationBuilder.AddColumn<int>(
                name: "BusinessToInvestId",
                table: "Investment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvestingBusinessId",
                table: "Investment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Investment_BusinessToInvestId",
                table: "Investment",
                column: "BusinessToInvestId");

            migrationBuilder.CreateIndex(
                name: "IX_Investment_InvestingBusinessId",
                table: "Investment",
                column: "InvestingBusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investment_Business_BusinessToInvestId",
                table: "Investment",
                column: "BusinessToInvestId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Investment_Business_InvestingBusinessId",
                table: "Investment",
                column: "InvestingBusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Business_BusinessToInvestId",
                table: "Investment");

            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Business_InvestingBusinessId",
                table: "Investment");

            migrationBuilder.DropIndex(
                name: "IX_Investment_BusinessToInvestId",
                table: "Investment");

            migrationBuilder.DropIndex(
                name: "IX_Investment_InvestingBusinessId",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "BusinessToInvestId",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "InvestingBusinessId",
                table: "Investment");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Investment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Investment_BusinessId",
                table: "Investment",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investment_Business_BusinessId",
                table: "Investment",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
