using System.Text.RegularExpressions;
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
        public StaticRuleCodes Code => StaticRuleCodes.MissingWhereDelete;
        public RecommendationCategory Category => RecommendationCategory.Safety;
        public Severity DefaultSeverity => Severity.Critical;

        private static readonly Regex StatementPattern = new(@"\b(?:DELETE|UPDATE)\b\s+([^\s;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex WherePattern = new(@"\bWHERE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex CommentPattern = new(@"(--[^\r\n]*|/\*.*?\*/)", RegexOptions.Singleline | RegexOptions.Compiled);

        public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Text)) return Task.FromResult<StaticCheckFinding?>(null);

            // remove comments to reduce false positives
            var cleaned = CommentPattern.Replace(query.Text, " ");

            // check for UPDATE/DELETE statements
            var matches = StatementPattern.Matches(cleaned);
            foreach (Match m in matches)
            {
                // try to isolate this statement to search for WHERE before next semicolon
                var startIndex = m.Index;
                var endIndex = cleaned.IndexOf(';', startIndex);
                var statement = endIndex >= 0 ? cleaned.Substring(startIndex, endIndex - startIndex) : cleaned.Substring(startIndex);

                // if there is no WHERE in this statement -> problem
                if (!WherePattern.IsMatch(statement))
                {
                    var msg = "DELETE или UPDATE без WHERE затронет все строки таблицы. Это опасная операция — подтвердите намерение или добавьте фильтр.";
                    return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
                }
            }

            return Task.FromResult<StaticCheckFinding?>(null);
        }
    }
}
