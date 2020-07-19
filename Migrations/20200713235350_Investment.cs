using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class Investment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Business_BusinessToInvestId",
                table: "Investment");

            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Business_InvestingBusinessId",
                table: "Investment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Investment",
                table: "Investment");

            migrationBuilder.RenameTable(
                name: "Investment",
                newName: "Investments");

            migrationBuilder.RenameIndex(
                name: "IX_Investment_InvestingBusinessId",
                table: "Investments",
                newName: "IX_Investments_InvestingBusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Investment_BusinessToInvestId",
                table: "Investments",
                newName: "IX_Investments_BusinessToInvestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Investments",
                table: "Investments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Business_BusinessToInvestId",
                table: "Investments",
                column: "BusinessToInvestId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Business_InvestingBusinessId",
                table: "Investments",
                column: "InvestingBusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_Business_BusinessToInvestId",
                table: "Investments");

            migrationBuilder.DropForeignKey(
                name: "FK_Investments_Business_InvestingBusinessId",
                table: "Investments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Investments",
                table: "Investments");

            migrationBuilder.RenameTable(
                name: "Investments",
                newName: "Investment");

            migrationBuilder.RenameIndex(
                name: "IX_Investments_InvestingBusinessId",
                table: "Investment",
                newName: "IX_Investment_InvestingBusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Investments_BusinessToInvestId",
                table: "Investment",
                newName: "IX_Investment_BusinessToInvestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Investment",
                table: "Investment",
                column: "Id");

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
    }
}
