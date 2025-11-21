using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Participant1Id = table.Column<string>(type: "text", nullable: true),
                    Participant2Id = table.Column<string>(type: "text", nullable: true),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatChannels_AspNetUsers_Participant1Id",
                        column: x => x.Participant1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatChannels_AspNetUsers_Participant2Id",
                        column: x => x.Participant2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatChannels_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatChannels_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatUserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DirectMessagesEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatUserSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    BannedById = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MutedById = table.Column<string>(type: "text", nullable: false),
                    MutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMutes_AspNetUsers_MutedById",
                        column: x => x.MutedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMutes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMutes_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMentions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMentions_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessageMedia",
                columns: table => new
                {
                    AttachmentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMessageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageMedia", x => new { x.AttachmentsId, x.ChatMessageId });
                    table.ForeignKey(
                        name: "FK_ChatMessageMedia_ChatMessages_ChatMessageId",
                        column: x => x.ChatMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessageMedia_Media_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "Media",
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

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_CommunityId",
                table: "ChatChannels",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_EventId",
                table: "ChatChannels",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_Participant1Id",
                table: "ChatChannels",
                column: "Participant1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_Participant2Id",
                table: "ChatChannels",
                column: "Participant2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMentions_MessageId",
                table: "ChatMentions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMentions_UserId",
                table: "ChatMentions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageMedia_ChatMessageId",
                table: "ChatMessageMedia",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_AuthorId",
                table: "ChatMessages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChannelId",
                table: "ChatMessages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMutes_ChannelId",
                table: "ChatMutes",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMutes_MutedById",
                table: "ChatMutes",
                column: "MutedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMutes_UserId",
                table: "ChatMutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUserSettings_UserId",
                table: "ChatUserSettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatBans");

            migrationBuilder.DropTable(
                name: "ChatMentions");

            migrationBuilder.DropTable(
                name: "ChatMessageMedia");

            migrationBuilder.DropTable(
                name: "ChatMutes");

            migrationBuilder.DropTable(
                name: "ChatUserSettings");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatChannels");
        }
    }
}
