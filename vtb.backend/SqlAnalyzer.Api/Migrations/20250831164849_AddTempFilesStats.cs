using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTempFilesStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TempFilesStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TempFiles = table.Column<long>(type: "bigint", nullable: false),
                    TempBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempFilesStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempFilesStats");
        }
    }
}
