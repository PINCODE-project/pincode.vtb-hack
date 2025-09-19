using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
    /// Проверка сравнения литералов без явного приведения типов (возможные implicit casts).
    /// Детектирует шаблоны: сравнение с UUID-литералом, датой 'YYYY-MM-DD' или timestamp-подобной строкой без ::type.
    /// </summary>
    public sealed class TypeMismatchComparisonRule //: IStaticRule
    {
        /// <inheritdoc />
        public StaticRules Code => StaticRules.TypeMismatchComparison;

        /// <inheritdoc />
        public Severity Severity => Severity.Warning;

        private static readonly Regex UuidLikeLiteral = new(@"=\s*'?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}'?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DateLikeLiteral = new(@"=\s*'?\d{4}-\d{2}-\d{2}(?:\s+\d{2}:\d{2}:\d{2}(?:\.\d+)?)?'?", RegexOptions.Compiled);
        private static readonly Regex NumericStringComparison = new(@"\b\w+\s*=\s*'\d+'\b", RegexOptions.Compiled);

        /// <inheritdoc />
        public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
        {
            if (UuidLikeLiteral.IsMatch(query.Text))
            {
                return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                    Code,
                    Severity,
                    StaticRuleProblemsDescriptions.TypeMismatchUuidComparisonProblemDescription,
                    StaticRuleRecommendations.TypeMismatchUuidComparisonRecommendation
                )); }

            if (DateLikeLiteral.IsMatch(query.Text))
            {
                 return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                    Code,
                    Severity,
                    StaticRuleProblemsDescriptions.TypeMismatchDateComparisonProblemDescription,
                    StaticRuleRecommendations.TypeMismatchDateComparisonRecommendation
                )); }

            if (NumericStringComparison.IsMatch(query.Text))
            {
                return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                    Code,
                    Severity,
                    StaticRuleProblemsDescriptions.TypeMismatchNumericComparisonProblemDescription,
                    StaticRuleRecommendations.TypeMismatchNumericComparisonRecommendation
                )); }

            return Task.FromResult<StaticAnalysisPoint?>(null);
        }
    }