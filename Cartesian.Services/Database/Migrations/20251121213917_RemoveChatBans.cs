using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChatBans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatBans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BannedById = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatBans_AspNetUsers_BannedById",
                        column: x => x.BannedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatBans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatBans_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatBans_BannedById",
                table: "ChatBans",
                column: "BannedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChatBans_ChannelId",
                table: "ChatBans",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatBans_UserId",
                table: "ChatBans",
                column: "UserId");
        }
    }
}
