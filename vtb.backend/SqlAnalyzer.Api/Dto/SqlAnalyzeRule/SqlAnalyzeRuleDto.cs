using SqlAnalyzer.Api.Dal.Constants;

namespace SqlAnalyzer.Api.Dto.SqlAnalyzeRule;

public class SqlAnalyzeRuleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required Severity Severity { get; init; }
    public required string Problem { get; init; }
    public required string Recommendation { get; init; }
    public required string Regex { get; init; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; init; }
}