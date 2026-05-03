using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReportGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Reports");
        }
    }
}
