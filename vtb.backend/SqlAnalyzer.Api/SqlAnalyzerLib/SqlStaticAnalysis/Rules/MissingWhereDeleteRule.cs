using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка DELETE/UPDATE без WHERE — критическая ошибка безопасности.
/// </summary>
public sealed class MissingWhereDeleteRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.MissingWhereDelete;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Critical;

    private static readonly Regex Pattern = new(@"\b(?:DELETE|UPDATE)\s+\w+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WherePattern = new(@"\bWHERE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text) && !WherePattern.IsMatch(query.Text))
        {
            var msg = "DELETE или UPDATE без WHERE затронет все строки таблицы. Это опасная операция — подтвердите намерение или добавьте фильтр.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}