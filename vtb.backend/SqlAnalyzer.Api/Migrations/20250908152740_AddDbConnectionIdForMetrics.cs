using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDbConnectionIdForMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DbConnectionId",
                table: "TempFilesStats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DbConnectionId",
                table: "TableStatictics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DbConnectionId",
                table: "IndexMetrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DbConnectionId",
                table: "CacheHitStats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DbConnectionId",
                table: "AutovacuumStats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DbConnectionId",
                table: "TempFilesStats");

            migrationBuilder.DropColumn(
                name: "DbConnectionId",
                table: "TableStatictics");

            migrationBuilder.DropColumn(
                name: "DbConnectionId",
                table: "IndexMetrics");

            migrationBuilder.DropColumn(
                name: "DbConnectionId",
                table: "CacheHitStats");

            migrationBuilder.DropColumn(
                name: "DbConnectionId",
                table: "AutovacuumStats");
        }
    }
}
