using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaName = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    IndexName = table.Column<string>(type: "text", nullable: false),
                    IndexScans = table.Column<long>(type: "bigint", nullable: false),
                    TuplesRead = table.Column<long>(type: "bigint", nullable: false),
                    TuplesFetched = table.Column<long>(type: "bigint", nullable: false),
                    IndexSize = table.Column<string>(type: "text", nullable: false),
                    IndexEfficiency = table.Column<double>(type: "double precision", nullable: false),
                    IndexStatus = table.Column<string>(type: "text", nullable: false),
                    BloatFactor = table.Column<double>(type: "double precision", nullable: false),
                    SequentialScans = table.Column<long>(type: "bigint", nullable: false),
                    SeqScanRatio = table.Column<double>(type: "double precision", nullable: false),
                    LiveTuples = table.Column<long>(type: "bigint", nullable: false),
                    DeadTuples = table.Column<long>(type: "bigint", nullable: false),
                    DeadTupleRatio = table.Column<double>(type: "double precision", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexMetrics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexMetrics");
        }
    }
}
