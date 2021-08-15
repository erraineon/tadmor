using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Add_Twitter_Media : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.PrimaryKey("PK_TwitterMedia", x => new { x.TweetId, x.MediaId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TwitterMedia");
        }
    }
}
