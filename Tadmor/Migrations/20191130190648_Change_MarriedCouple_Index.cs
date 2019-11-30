using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Change_MarriedCouple_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id",
                table: "MarriedCouples");

            migrationBuilder.CreateIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id_GuildId",
                table: "MarriedCouples",
                columns: new[] { "Partner1Id", "Partner2Id", "GuildId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id_GuildId",
                table: "MarriedCouples");

            migrationBuilder.CreateIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id",
                table: "MarriedCouples",
                columns: new[] { "Partner1Id", "Partner2Id" },
                unique: true);
        }
    }
}
