using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class convertToDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "UnlocksAtTotalEarnings",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "PerOwnedModifier",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "EspionageModifier",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "EspionageDefenseModifier",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Cost",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "CashModifier",
                table: "Purchasables",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "InvestmentAmount",
                table: "Investments",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "InvestedBusinessCashPerSecondAtInvestment",
                table: "Investments",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<double>(
                name: "EspionageDefense",
                table: "Business",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "EspionageChance",
                table: "Business",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "CashPerSecond",
                table: "Business",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "UnlocksAtTotalEarnings",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "PerOwnedModifier",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "EspionageModifier",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "EspionageDefenseModifier",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "Cost",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "CashModifier",
                table: "Purchasables",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "InvestmentAmount",
                table: "Investments",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "InvestedBusinessCashPerSecondAtInvestment",
                table: "Investments",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                type: "timestamp(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<float>(
                name: "EspionageDefense",
                table: "Business",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "EspionageChance",
                table: "Business",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<float>(
                name: "CashPerSecond",
                table: "Business",
                type: "float",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
