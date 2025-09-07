using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDbConnectionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DbConnections",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "DbConnections");
        }
    }
}
