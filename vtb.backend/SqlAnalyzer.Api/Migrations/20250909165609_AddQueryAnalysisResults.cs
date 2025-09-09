using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryAnalysisResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QueryAnalysis",
                table: "QueryAnalysis");

            migrationBuilder.RenameTable(
                name: "QueryAnalysis",
                newName: "Queries");

            migrationBuilder.RenameColumn(
                name: "Query",
                table: "Queries",
                newName: "Sql");

            migrationBuilder.RenameColumn(
                name: "AnalyzeResult",
                table: "Queries",
                newName: "ExplainResult");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queries",
                table: "Queries",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "QueryAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recommendations = table.Column<string>(type: "jsonb", nullable: false),
                    LlmRecommendations = table.Column<string>(type: "jsonb", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryAnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryAnalysisResults_Queries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "Queries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueryAnalysisResults_QueryId",
                table: "QueryAnalysisResults",
                column: "QueryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueryAnalysisResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queries",
                table: "Queries");

            migrationBuilder.RenameTable(
                name: "Queries",
                newName: "QueryAnalysis");

            migrationBuilder.RenameColumn(
                name: "Sql",
                table: "QueryAnalysis",
                newName: "Query");

            migrationBuilder.RenameColumn(
                name: "ExplainResult",
                table: "QueryAnalysis",
                newName: "AnalyzeResult");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueryAnalysis",
                table: "QueryAnalysis",
                column: "Id");
        }
    }
}
