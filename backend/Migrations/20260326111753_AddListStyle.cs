using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReportGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddListStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ListStyle",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListStyle",
                table: "Reports");
        }
    }
}
