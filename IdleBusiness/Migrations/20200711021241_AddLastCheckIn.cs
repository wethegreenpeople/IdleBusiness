using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdleBusiness.Migrations
{
    public partial class AddLastCheckIn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckIn",
                table: "Business",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCheckIn",
                table: "Business");
        }
    }
}
