using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
    /// Проверка сравнения литералов без явного приведения типов (возможные implicit casts).
    /// Детектирует шаблоны: сравнение с UUID-литералом, датой 'YYYY-MM-DD' или timestamp-подобной строкой без ::type.
    /// </summary>
    public sealed class TypeMismatchComparisonRule : IStaticRule
    {
        /// <inheritdoc />
        public StaticRuleCodes Code => StaticRuleCodes.TypeMismatchComparison;

        /// <inheritdoc />
        public RecommendationCategory Category => RecommendationCategory.Safety;

        /// <inheritdoc />
        public Severity DefaultSeverity => Severity.Medium;

        private static readonly Regex UuidLikeLiteral = new(@"=\s*'?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}'?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DateLikeLiteral = new(@"=\s*'?\d{4}-\d{2}-\d{2}(?:\s+\d{2}:\d{2}:\d{2}(?:\.\d+)?)?'?", RegexOptions.Compiled);
        private static readonly Regex NumericStringComparison = new(@"\b\w+\s*=\s*'\d+'\b", RegexOptions.Compiled);

        /// <inheritdoc />
        public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
        {
            if (UuidLikeLiteral.IsMatch(query.Text))
            {
                var msg = "Найдено сравнение с UUID-форматной строкой без явного ::uuid. Явное приведение ('...')::uuid предпочтительнее, чтобы избежать implicit cast и ошибок планирования.";
                return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
            }

            if (DateLikeLiteral.IsMatch(query.Text))
            {
                var msg = "Найдено сравнение с датой/временем в виде строки без явного типа. Используйте DATE 'YYYY-MM-DD' или TIMESTAMP 'YYYY-MM-DD HH:MM:SS' для точности и избежания implicit casts.";
                return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
            }

            if (NumericStringComparison.IsMatch(query.Text))
            {
                var msg = "Найдено сравнение колонки с численной строкой (например, col = '123'). Рассмотрите приведение литерала к числу или хранение как числового типа.";
                return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, Severity.Low, new List<string>()));
            }

            return Task.FromResult<StaticCheckFinding?>(null);
        }
    }