using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddSendingBusiness : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateReceived",
                table: "Messages",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SendingBusinessId",
                table: "Messages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SendingBusinessId",
                table: "Messages",
                column: "SendingBusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Business_SendingBusinessId",
                table: "Messages",
                column: "SendingBusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Business_SendingBusinessId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SendingBusinessId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DateReceived",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SendingBusinessId",
                table: "Messages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "Business",
                type: "timestamp(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }
    }
}
