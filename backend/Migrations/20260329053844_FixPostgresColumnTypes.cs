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
            // Only run for PostgreSQL - convert TEXT columns to proper types
            migrationBuilder.Sql(@"
                -- Check if columns are TEXT type (SQLite format) and convert to PostgreSQL types
                DO $$ 
                BEGIN
                    -- Fix CreatedAt column
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Reports' AND column_name = 'CreatedAt' 
                        AND data_type = 'text'
                    ) THEN
                        ALTER TABLE ""Reports"" ALTER COLUMN ""CreatedAt"" TYPE timestamp USING ""CreatedAt""::timestamp;
                    END IF;
                    
                    -- Fix Date column
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Reports' AND column_name = 'Date' 
                        AND data_type = 'text'
                    ) THEN
                        ALTER TABLE ""Reports"" ALTER COLUMN ""Date"" TYPE timestamp USING ""Date""::timestamp;
                    END IF;
                    
                    -- Fix TimeIn column
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Reports' AND column_name = 'TimeIn' 
                        AND data_type = 'text'
                    ) THEN
                        ALTER TABLE ""Reports"" ALTER COLUMN ""TimeIn"" TYPE interval USING ""TimeIn""::interval;
                    END IF;
                    
                    -- Fix TimeOut column
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Reports' AND column_name = 'TimeOut' 
                        AND data_type = 'text'
                    ) THEN
                        ALTER TABLE ""Reports"" ALTER COLUMN ""TimeOut"" TYPE interval USING ""TimeOut""::interval;
                    END IF;
                    
                    -- Fix IsPremium column
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Templates' AND column_name = 'IsPremium' 
                        AND data_type = 'integer'
                    ) THEN
                        ALTER TABLE ""Templates"" ALTER COLUMN ""IsPremium"" TYPE boolean USING (""IsPremium"" != 0);
                    END IF;
                END $$;
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
