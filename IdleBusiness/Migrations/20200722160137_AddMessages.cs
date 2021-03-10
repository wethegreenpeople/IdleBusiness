using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageBody = table.Column<string>(nullable: true),
                    ReadByBusiness = table.Column<bool>(nullable: false),
                    ReceivingBusinessId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Business_ReceivingBusinessId",
                        column: x => x.ReceivingBusinessId,
                        principalTable: "Business",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceivingBusinessId",
                table: "Messages",
                column: "ReceivingBusinessId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchasables_Purchasables_PurchasableUpgradeId",
                table: "Purchasables");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Purchasables_PurchasableUpgradeId",
                table: "Purchasables");

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
                name: "IX_Purchasables_PurchasableUpgradeId",
                table: "Purchasables",
                column: "PurchasableUpgradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchasables_Purchasables_PurchasableUpgradeId",
                table: "Purchasables",
                column: "PurchasableUpgradeId",
                principalTable: "Purchasables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
