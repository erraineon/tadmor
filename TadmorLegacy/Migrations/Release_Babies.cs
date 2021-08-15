using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Release_Babies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Babies",
                nullable: false,
                defaultValue: DateTime.Now);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Babies");
        }
    }
}
