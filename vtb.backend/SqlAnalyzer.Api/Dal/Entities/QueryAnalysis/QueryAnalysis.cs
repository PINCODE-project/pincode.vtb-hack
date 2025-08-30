using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

public class QueryAnalysis : EntityBase, IEntityCreatedAt
{
    public required Guid DbConnectionId { get; set; }
    public required string Query { get; set; }
    public string? AnalyzeResult { get; set; }
    public DateTime CreateAt { get; set; }
}