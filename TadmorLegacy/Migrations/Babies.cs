using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Babies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "KissCooldown",
                table: "MarriedCouples",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<float>(
                name: "FloatKisses",
                table: "MarriedCouples",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "Babies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    MarriedCoupleId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Babies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Babies_MarriedCouples_MarriedCoupleId",
                        column: x => x.MarriedCoupleId,
                        principalTable: "MarriedCouples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Babies_MarriedCoupleId",
                table: "Babies",
                column: "MarriedCoupleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Babies");

            migrationBuilder.DropColumn(
                name: "KissCooldown",
                table: "MarriedCouples");

            migrationBuilder.DropColumn(
                name: "FloatKisses",
                table: "MarriedCouples");
        }
    }
}
