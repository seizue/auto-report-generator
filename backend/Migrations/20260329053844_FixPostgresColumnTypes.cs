using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReportGenerator.Migrations
{
    /// <inheritdoc />
    public partial class FixPostgresColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only run this for PostgreSQL, not SQLite
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Check if we're using PostgreSQL (not SQLite)
                    IF EXISTS (SELECT 1 FROM pg_catalog.pg_tables WHERE schemaname = 'public' AND tablename = 'Reports') THEN
                        -- Convert CreatedAt from TEXT to TIMESTAMP
                        ALTER TABLE ""Reports"" 
                        ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone 
                        USING ""CreatedAt""::timestamp without time zone;
                        
                        -- Convert Date from TEXT to TIMESTAMP
                        ALTER TABLE ""Reports"" 
                        ALTER COLUMN ""Date"" TYPE timestamp without time zone 
                        USING ""Date""::timestamp without time zone;
                        
                        -- Convert TimeIn from TEXT to INTERVAL
                        ALTER TABLE ""Reports"" 
                        ALTER COLUMN ""TimeIn"" TYPE interval 
                        USING ""TimeIn""::interval;
                        
                        -- Convert TimeOut from TEXT to INTERVAL
                        ALTER TABLE ""Reports"" 
                        ALTER COLUMN ""TimeOut"" TYPE interval 
                        USING ""TimeOut""::interval;
                        
                        -- Convert IsPremium from INTEGER to BOOLEAN
                        ALTER TABLE ""Templates"" 
                        ALTER COLUMN ""IsPremium"" TYPE boolean 
                        USING CASE WHEN ""IsPremium"" = 0 THEN false ELSE true END;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
