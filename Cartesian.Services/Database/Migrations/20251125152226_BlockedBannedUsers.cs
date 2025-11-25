using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class BlockedBannedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // In some development databases a previous failed migration or manual operation
            // can leave behind a PostgreSQL composite type with the same name as a table
            // (pg_type.typname). When EF Core later attempts to create the table the
            // implicit creation of the composite type conflicts with the existing type
            // and causes `duplicate key value violates unique constraint "pg_type_typname_nsp_index"`.
            // To be robust for local/dev environments we drop any lingering types here
            // before creating the table. This is safe for development only — in production
            // you should avoid ad-hoc types or manually inspect the database.
            migrationBuilder.Sql(@"
DO $$
BEGIN
   IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'communitybans') THEN
       EXECUTE 'DROP TYPE IF EXISTS ""CommunityBans"" CASCADE';
   END IF;
END$$;
");

            migrationBuilder.CreateTable(
                name: "CommunityBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    BannedById = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityBans_AspNetUsers_BannedById",
                        column: x => x.BannedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityBans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityBans_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Also drop a lingering type for UserBlocks if present; helps avoid the same
            // error when creating the table in development DBs where a previous run
            // may have left the type behind.
            migrationBuilder.Sql(@"
DO $$
BEGIN
   IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'userblocks') THEN
       EXECUTE 'DROP TYPE IF EXISTS ""UserBlocks"" CASCADE';
   END IF;
END$$;
");

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerId = table.Column<string>(type: "text", nullable: false),
                    BlockedId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockedId",
                        column: x => x.BlockedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityBans_BannedById",
                table: "CommunityBans",
                column: "BannedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityBans_CommunityId_UserId",
                table: "CommunityBans",
                columns: new[] { "CommunityId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityBans_UserId",
                table: "CommunityBans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockedId",
                table: "UserBlocks",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockerId_BlockedId",
                table: "UserBlocks",
                columns: new[] { "BlockerId", "BlockedId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityBans");

            migrationBuilder.DropTable(
                name: "UserBlocks");
        }
    }
}
