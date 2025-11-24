using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartesianUserEvent1_AspNetUsers_ParticipantsId",
                table: "CartesianUserEvent1");

            migrationBuilder.DropForeignKey(
                name: "FK_CartesianUserEvent1_Events_ParticipatedEventsId",
                table: "CartesianUserEvent1");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartesianUserEvent1",
                table: "CartesianUserEvent1");

            migrationBuilder.RenameTable(
                name: "CartesianUserEvent1",
                newName: "EventFavorites");

            migrationBuilder.RenameColumn(
                name: "ParticipatedEventsId",
                table: "EventFavorites",
                newName: "FavoritedEventsId");

            migrationBuilder.RenameColumn(
                name: "ParticipantsId",
                table: "EventFavorites",
                newName: "FavoritedById");

            migrationBuilder.RenameIndex(
                name: "IX_CartesianUserEvent1_ParticipatedEventsId",
                table: "EventFavorites",
                newName: "IX_EventFavorites_FavoritedEventsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFavorites",
                table: "EventFavorites",
                columns: new[] { "FavoritedById", "FavoritedEventsId" });

            migrationBuilder.CreateTable(
                name: "CartesianUserEvent2",
                columns: table => new
                {
                    ParticipantsId = table.Column<string>(type: "text", nullable: false),
                    ParticipatedEventsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartesianUserEvent2", x => new { x.ParticipantsId, x.ParticipatedEventsId });
                    table.ForeignKey(
                        name: "FK_CartesianUserEvent2_AspNetUsers_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartesianUserEvent2_Events_ParticipatedEventsId",
                        column: x => x.ParticipatedEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartesianUserEvent2_ParticipatedEventsId",
                table: "CartesianUserEvent2",
                column: "ParticipatedEventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFavorites_AspNetUsers_FavoritedById",
                table: "EventFavorites",
                column: "FavoritedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFavorites_Events_FavoritedEventsId",
                table: "EventFavorites",
                column: "FavoritedEventsId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFavorites_AspNetUsers_FavoritedById",
                table: "EventFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFavorites_Events_FavoritedEventsId",
                table: "EventFavorites");

            migrationBuilder.DropTable(
                name: "CartesianUserEvent2");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFavorites",
                table: "EventFavorites");

            migrationBuilder.RenameTable(
                name: "EventFavorites",
                newName: "CartesianUserEvent1");

            migrationBuilder.RenameColumn(
                name: "FavoritedEventsId",
                table: "CartesianUserEvent1",
                newName: "ParticipatedEventsId");

            migrationBuilder.RenameColumn(
                name: "FavoritedById",
                table: "CartesianUserEvent1",
                newName: "ParticipantsId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFavorites_FavoritedEventsId",
                table: "CartesianUserEvent1",
                newName: "IX_CartesianUserEvent1_ParticipatedEventsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartesianUserEvent1",
                table: "CartesianUserEvent1",
                columns: new[] { "ParticipantsId", "ParticipatedEventsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartesianUserEvent1_AspNetUsers_ParticipantsId",
                table: "CartesianUserEvent1",
                column: "ParticipantsId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartesianUserEvent1_Events_ParticipatedEventsId",
                table: "CartesianUserEvent1",
                column: "ParticipatedEventsId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
