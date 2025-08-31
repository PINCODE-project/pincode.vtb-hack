using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAutovacuumStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutovacuumStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SchemaName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LiveTuples = table.Column<long>(type: "bigint", nullable: false),
                    DeadTuples = table.Column<long>(type: "bigint", nullable: false),
                    DeadTupleRatio = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TableSize = table.Column<long>(type: "bigint", nullable: false),
                    LastVacuum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAutoVacuum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChangeRatePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutovacuumStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutovacuumStats");
        }
    }
}
