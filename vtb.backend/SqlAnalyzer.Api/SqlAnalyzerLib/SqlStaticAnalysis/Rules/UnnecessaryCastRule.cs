using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class UnnecessaryCastRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.UnnecessaryCast;
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    public Severity DefaultSeverity => Severity.Low;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text;

        if (sql.Contains("::text::text", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Найдены избыточные приведения типов (::text::text).",
                Category,
                DefaultSeverity,
                new List<string> { "::cast" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
