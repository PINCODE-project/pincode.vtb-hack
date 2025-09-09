using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzer.Api.Services.LlmClient.Data;
using SqlAnalyzerLib.Recommendation.Models;

namespace SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

public class QueryAnalysisResult : EntityBase, IEntityCreatedAt
{
    public QueryAnalysis Query { get; init; } = null!;
    public required Guid QueryId { get; init; }

    public IReadOnlyCollection<Recommendation> Recommendations { get; init; } = [];
    public LlmAnswer? LlmRecommendations { get; init; } = null;
    public DateTime CreateAt { get; set; }
}