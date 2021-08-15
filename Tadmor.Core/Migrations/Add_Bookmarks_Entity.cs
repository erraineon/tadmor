using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Core.Migrations
{
    public partial class Add_Bookmarks_Entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    ChatClientId = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    LastSeenValue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => new { x.ChatClientId, x.GuildId, x.ChannelId, x.Key });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");
        }
    }
}
