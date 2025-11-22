using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create GIN indexes for full-text search with simple configuration
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_events_search 
                ON ""Events"" 
                USING GIN (to_tsvector('simple', ""Name"" || ' ' || ""Description""));
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_communities_search 
                ON ""Communities"" 
                USING GIN (to_tsvector('simple', ""Name"" || ' ' || ""Description""));
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_users_search 
                ON ""AspNetUsers"" 
                USING GIN (to_tsvector('simple', COALESCE(""UserName"", '')));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_events_search;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_communities_search;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_users_search;");
        }
    }
}
