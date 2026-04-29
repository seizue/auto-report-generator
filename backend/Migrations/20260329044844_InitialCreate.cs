using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoReportGenerator.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<string>(nullable: false, defaultValue: ""),
                    Name = table.Column<string>(nullable: false),
                    Department = table.Column<string>(nullable: false),
                    Date = isSqlite
                        ? table.Column<DateTime>(type: "TEXT", nullable: false)
                        : table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimeIn = isSqlite
                        ? table.Column<TimeSpan>(type: "TEXT", nullable: false)
                        : table.Column<TimeSpan>(type: "interval", nullable: false),
                    TimeOut = isSqlite
                        ? table.Column<TimeSpan>(type: "TEXT", nullable: false)
                        : table.Column<TimeSpan>(type: "interval", nullable: false),
                    Notes = table.Column<string>(nullable: false),
                    TemplateType = table.Column<string>(nullable: false),
                    ListStyle = table.Column<string>(nullable: false),
                    CreatedAt = isSqlite
                        ? table.Column<DateTime>(type: "TEXT", nullable: false)
                        : table.Column<DateTime>(type: "timestamp", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IsPremium = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportId = table.Column<int>(nullable: false),
                    Task = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportItems_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Description", "IsPremium", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "Track your daily tasks and accomplishments.", false, "Daily Accomplishment Report", "daily" },
                    { 2, "Summarize your week's work and progress.", false, "Weekly Summary Report", "weekly" },
                    { 3, "Detailed time-based work log.", false, "Work Log Report", "worklog" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportItems_ReportId",
                table: "ReportItems",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReportItems");
            migrationBuilder.DropTable(name: "Templates");
            migrationBuilder.DropTable(name: "Reports");
        }
    }
}
