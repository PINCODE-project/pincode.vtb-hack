using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

public class QueryAnalysis : EntityBase, IEntityCreatedAt
{
    public required Guid DbConnectionId { get; set; }
    public required string Sql { get; set; }
    public ExplainRootPlan? ExplainResult { get; set; }
    public DateTime CreateAt { get; set; }
}