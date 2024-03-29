﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Tadmor.Core.Migrations
{
    public partial class _20201121023636_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "GuildPreferences",
                table => new
                {
                    GuildId = table.Column<ulong>("INTEGER", nullable: false),
                    Preferences = table.Column<string>("TEXT", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_GuildPreferences", x => x.GuildId); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "GuildPreferences");
        }
    }
}