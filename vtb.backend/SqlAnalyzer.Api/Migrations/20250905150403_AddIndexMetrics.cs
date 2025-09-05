using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexMetrics : Migration
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
                    IndexSize = table.Column<long>(type: "bigint", nullable: false),
                    TuplesRead = table.Column<long>(type: "bigint", nullable: false),
                    TuplesFetched = table.Column<long>(type: "bigint", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableStatictics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaName = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    CountSeqScan = table.Column<long>(type: "bigint", nullable: false),
                    TuplesReadCountSeqScan = table.Column<long>(type: "bigint", nullable: false),
                    IndexCountSeqScan = table.Column<long>(type: "bigint", nullable: false),
                    TuplesFetchedIndexScan = table.Column<long>(type: "bigint", nullable: false),
                    IndexUsageRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableStatictics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexMetrics");

            migrationBuilder.DropTable(
                name: "TableStatictics");
        }
    }
}
