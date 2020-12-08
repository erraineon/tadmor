using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Migrations
{
    public partial class Add_Upvotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Babies");

            migrationBuilder.DropTable(
                name: "MarriedCouples");

            migrationBuilder.CreateTable(
                name: "Upvotes",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    UpvotesCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Upvotes", x => new { x.GuildId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Upvotes");

            migrationBuilder.CreateTable(
                name: "MarriedCouples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    KissCooldown = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    FloatKisses = table.Column<float>(type: "REAL", nullable: false),
                    Kisses = table.Column<int>(type: "INTEGER", nullable: false),
                    LastKissed = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Partner1Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Partner2Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriedCouples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Babies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    MarriedCoupleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_MarriedCouples_Partner1Id_Partner2Id_GuildId",
                table: "MarriedCouples",
                columns: new[] { "Partner1Id", "Partner2Id", "GuildId" },
                unique: true);
        }
    }
}
