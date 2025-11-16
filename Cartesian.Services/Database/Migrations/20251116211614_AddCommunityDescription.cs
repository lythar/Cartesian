using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Communities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Communities");
        }
    }
}
