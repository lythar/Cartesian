using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Media_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_Media_AvatarId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_AvatarId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "CommunityId",
                table: "Media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventWindowId",
                table: "Media",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_CommunityId",
                table: "Media",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_EventId",
                table: "Media",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_EventWindowId",
                table: "Media",
                column: "EventWindowId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_AvatarId",
                table: "Communities",
                column: "AvatarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Media_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId",
                principalTable: "Media",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_Media_AvatarId",
                table: "Communities",
                column: "AvatarId",
                principalTable: "Media",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Communities_CommunityId",
                table: "Media",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Events_EventId",
                table: "Media",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_EventWindows_EventWindowId",
                table: "Media",
                column: "EventWindowId",
                principalTable: "EventWindows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Media_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_Media_AvatarId",
                table: "Communities");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Communities_CommunityId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Events_EventId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_EventWindows_EventWindowId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_CommunityId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_EventId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_EventWindowId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Communities_AvatarId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "EventWindowId",
                table: "Media");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_AvatarId",
                table: "Communities",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Media_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId",
                principalTable: "Media",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_Media_AvatarId",
                table: "Communities",
                column: "AvatarId",
                principalTable: "Media",
                principalColumn: "Id");
        }
    }
}
