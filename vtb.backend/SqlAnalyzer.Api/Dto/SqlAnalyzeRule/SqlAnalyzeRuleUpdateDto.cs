using SqlAnalyzer.Api.Dal.Constants;

namespace SqlAnalyzer.Api.Dto.SqlAnalyzeRule;

public class SqlAnalyzeRuleUpdateDto
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public Severity? Severity { get; init; }
    public string? Problem { get; init; }
    public string? Recommendation { get; init; }
    public string? Regex { get; init; }
    public bool? IsActive { get; set; }
}