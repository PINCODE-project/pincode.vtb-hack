using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules
{
    /// <summary>
    /// Проверка DELETE/UPDATE без WHERE — критическая ошибка безопасности.
    /// Базовая, но улучшенная: учитывает комментарии и ищет WHERE в том же операторе.
    /// </summary>
    public sealed class MissingWhereDeleteRule : IStaticRule
    {
        public StaticRules Code => StaticRules.MissingWhereDelete;
        public Severity Severity => Severity.Critical;

        private static readonly Regex StatementPattern = new(@"\b(?:DELETE|UPDATE)\b\s+([^\s;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex WherePattern = new(@"\bWHERE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex CommentPattern = new(@"(--[^\r\n]*|/\*.*?\*/)", RegexOptions.Singleline | RegexOptions.Compiled);

        public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Text)) return Task.FromResult<StaticAnalysisPoint?>(null);

            var cleaned = CommentPattern.Replace(query.Text, " ");

            var matches = StatementPattern.Matches(cleaned);
            foreach (Match m in matches)
            {
                var startIndex = m.Index;
                var endIndex = cleaned.IndexOf(';', startIndex);
                var statement = endIndex >= 0 ? cleaned.Substring(startIndex, endIndex - startIndex) : cleaned.Substring(startIndex);

                if (!WherePattern.IsMatch(statement))
                {
                    return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                        Code,
                        Severity,
                        StaticRuleProblemsDescriptions.MissingWhereDeleteProblemDescription,
                        StaticRuleRecommendations.MissingWhereDeleteRecommendation
                    ));
                }
            }

            return Task.FromResult<StaticAnalysisPoint?>(null);
        }
    }
}
