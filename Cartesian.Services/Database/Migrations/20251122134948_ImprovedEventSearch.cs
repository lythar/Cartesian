using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cartesian.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedEventSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old index
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_events_search;");

            // Create an immutable function to convert integer array to text
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION int_array_to_text(integer[])
                RETURNS text AS $$
                    SELECT string_agg(x::text, ' ') FROM unnest($1) AS x
                $$ LANGUAGE SQL IMMUTABLE;
            ");

            // Create improved index that includes Tags converted to text
            migrationBuilder.Sql(@"
                CREATE INDEX idx_events_search 
                ON ""Events"" 
                USING GIN (
                    to_tsvector('simple', 
                        ""Name"" || ' ' || 
                        ""Description"" || ' ' || 
                        COALESCE(int_array_to_text(""Tags""), '')
                    )
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the original index
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_events_search;");

            migrationBuilder.Sql(@"
                CREATE INDEX idx_events_search 
                ON ""Events"" 
                USING GIN (to_tsvector('simple', ""Name"" || ' ' || ""Description""));
            ");

            // Drop the helper function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS int_array_to_text(integer[]);");
        }
    }
}
