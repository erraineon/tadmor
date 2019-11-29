using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Marriage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarriedCouples",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Partner1Id = table.Column<ulong>(nullable: false),
                    Partner2Id = table.Column<ulong>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriedCouples", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id",
                table: "MarriedCouples",
                columns: new[] { "Partner1Id", "Partner2Id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarriedCouples");
        }
    }
}
