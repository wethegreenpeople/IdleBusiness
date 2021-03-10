using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class ManyToMany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "PerOwnedModifier",
                table: "Purchasables",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "BusinessPurchases",
                columns: table => new
                {
                    BusinessId = table.Column<int>(nullable: false),
                    PurchaseId = table.Column<int>(nullable: false),
                    AmountOfPurchases = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPurchases", x => new { x.BusinessId, x.PurchaseId });
                    table.ForeignKey(
                        name: "FK_BusinessPurchases_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessPurchases_Purchasables_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchasables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPurchases_PurchaseId",
                table: "BusinessPurchases",
                column: "PurchaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessPurchases");

            migrationBuilder.DropColumn(
                name: "PerOwnedModifier",
                table: "Purchasables");
        }
    }
}
