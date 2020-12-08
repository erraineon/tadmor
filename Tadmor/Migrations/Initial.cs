using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildPreferences",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Preferences = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildPreferences", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildPreferences");
        }
    }
}
