using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Add_Upvotes_New : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Upvotes");
            migrationBuilder.CreateTable(
                name: "Upvotes",
                columns: table => new
                {
                    TargetUserId = table.Column<ulong>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    MessageId = table.Column<ulong>(nullable: false),
                    VoterId = table.Column<ulong>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Upvotes", x => new { x.GuildId, x.TargetUserId, x.MessageId, x.VoterId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
