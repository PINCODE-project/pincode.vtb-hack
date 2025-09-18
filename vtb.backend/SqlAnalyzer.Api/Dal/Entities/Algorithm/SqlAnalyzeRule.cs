using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Algorithm;

public class SqlAnalyzeRule : EntityBase, IEntityCreatedAt
{
    public required string Name { get; set; }
    public required Severity Severity { get; set; }
    public required string Problem { get; set; }
    public required string Recommendation { get; set; }
    public required string Regex { get; set; }
    public required bool IsActive { get; set; }
    public DateTime CreateAt { get; init; }
}