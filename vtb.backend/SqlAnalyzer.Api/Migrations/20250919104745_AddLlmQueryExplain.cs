using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmQueryExplain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LlmRecommendations",
                table: "QueryAnalysisResults",
                newName: "LlmResult");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LlmResult",
                table: "QueryAnalysisResults",
                newName: "LlmRecommendations");
        }
    }
}
