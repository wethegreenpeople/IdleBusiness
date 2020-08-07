using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class BusinessInvestments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Investments_Business_BusinessToInvestId",
            //    table: "Investments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Investments_Business_InvestingBusinessId",
            //    table: "Investments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Investments_Business_PartnerBusinessId",
            //    table: "Investments");

            migrationBuilder.DropIndex(
                name: "IX_Investments_BusinessToInvestId",
                table: "Investments");

            migrationBuilder.DropIndex(
                name: "IX_Investments_InvestingBusinessId",
                table: "Investments");

            migrationBuilder.DropIndex(
                name: "IX_Investments_PartnerBusinessId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "BusinessToInvestId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "InvestingBusinessId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "PartnerBusinessId",
                table: "Investments");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Investments",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateTable(
                name: "BusinessInvestments",
                columns: table => new
                {
                    BusinessId = table.Column<int>(nullable: false),
                    InvestmentId = table.Column<int>(nullable: false),
                    InvestmentType = table.Column<int>(nullable: false),
                    InvestmentDirection = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInvestments", x => new { x.BusinessId, x.InvestmentId });
                    table.ForeignKey(
                        name: "FK_BusinessInvestments_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessInvestments_Investments_InvestmentId",
                        column: x => x.InvestmentId,
                        principalTable: "Investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Investments_BusinessId",
                table: "Investments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvestments_InvestmentId",
                table: "BusinessInvestments",
                column: "InvestmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Business_BusinessId",
                table: "Investments",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_Business_BusinessId",
                table: "Investments");

            migrationBuilder.DropTable(
                name: "BusinessInvestments");

            migrationBuilder.DropIndex(
                name: "IX_Investments_BusinessId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Investments");

            migrationBuilder.AddColumn<int>(
                name: "BusinessToInvestId",
                table: "Investments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvestingBusinessId",
                table: "Investments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartnerBusinessId",
                table: "Investments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                type: "timestamp(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Investments_BusinessToInvestId",
                table: "Investments",
                column: "BusinessToInvestId");

            migrationBuilder.CreateIndex(
                name: "IX_Investments_InvestingBusinessId",
                table: "Investments",
                column: "InvestingBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Investments_PartnerBusinessId",
                table: "Investments",
                column: "PartnerBusinessId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Business_PartnerBusinessId",
                table: "Investments",
                column: "PartnerBusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
