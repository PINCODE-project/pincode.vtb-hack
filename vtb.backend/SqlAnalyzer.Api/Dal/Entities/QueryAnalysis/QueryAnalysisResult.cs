using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzer.Api.Dal.ValueObjects;

namespace SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

public class QueryAnalysisResult : EntityBase, IEntityCreatedAt
{
    public QueryAnalysis Query { get; init; } = null!;
    public required Guid QueryId { get; init; }

    public SqlAlgorithmAnalysisResult Recommendations { get; set; }
    public SqlLlmAnalysisResult? LlmResult { get; set; } = null;
    
    public List<Guid>? FindindCustomRules { get; set; } = [];
    public DateTime CreateAt { get; set; }
}