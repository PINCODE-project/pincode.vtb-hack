using SqlAnalyzer.Api.Dal.Constants;

namespace SqlAnalyzer.Api.Dto.SqlAnalyzeRule;

public class SqlAnalyzeRuleCreateDto
{
    public required string Name { get; init; }
    public required Severity Severity { get; init; }
    public required string Problem { get; init; }
    public required string Recommendation { get; init; }
    public required string Regex { get; init; }
}