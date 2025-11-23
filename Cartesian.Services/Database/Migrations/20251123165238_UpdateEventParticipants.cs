using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartesianUserEventWindow");

            migrationBuilder.CreateTable(
                name: "CartesianUserEvent1",
                columns: table => new
                {
                    ParticipantsId = table.Column<string>(type: "text", nullable: false),
                    ParticipatedEventsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartesianUserEvent1", x => new { x.ParticipantsId, x.ParticipatedEventsId });
                    table.ForeignKey(
                        name: "FK_CartesianUserEvent1_AspNetUsers_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartesianUserEvent1_Events_ParticipatedEventsId",
                        column: x => x.ParticipatedEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartesianUserEvent1_ParticipatedEventsId",
                table: "CartesianUserEvent1",
                column: "ParticipatedEventsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartesianUserEvent1");

            migrationBuilder.CreateTable(
                name: "CartesianUserEventWindow",
                columns: table => new
                {
                    ParticipantsId = table.Column<string>(type: "text", nullable: false),
                    ParticipatedWindowsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartesianUserEventWindow", x => new { x.ParticipantsId, x.ParticipatedWindowsId });
                    table.ForeignKey(
                        name: "FK_CartesianUserEventWindow_AspNetUsers_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartesianUserEventWindow_EventWindows_ParticipatedWindowsId",
                        column: x => x.ParticipatedWindowsId,
                        principalTable: "EventWindows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartesianUserEventWindow_ParticipatedWindowsId",
                table: "CartesianUserEventWindow",
                column: "ParticipatedWindowsId");
        }
    }
}
