using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class ExtendTwitterMediaPk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                 "TwitterMedia");

            migrationBuilder.CreateTable(
                name: "TwitterMedia",
                columns: table => new
                {
                    TweetId = table.Column<ulong>(nullable: false),
                    MediaId = table.Column<ulong>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    StatusText = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitterMedia", x => new { x.TweetId, x.MediaId, x.Username });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
