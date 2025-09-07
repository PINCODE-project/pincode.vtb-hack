using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.Recommendation.Enums;
using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.Recommendation.Models.Explain;
using SqlAnalyzerLib.Recommendation.Models.Query;

namespace SqlAnalyzerLib.Recommendation;

using Models;

/// <summary>
    /// Построитель рекомендаций по результатам анализа EXPLAIN JSON
    /// </summary>
    public class ExplainRecommendationProvider : IRecommendationProvider
    {
        public IEnumerable<Recommendation> BuildRecommendations(ExplainAnalysisResult analysisResult)
        {
            foreach (var issue in analysisResult.Issues)
            {
                yield return new Recommendation
                {
                    Category = MapCategory(issue.Rule),
                    Severity = MapSeverity(issue.Severity),
                    Message = issue.Description,
                    Suggestion = issue.Recommendation
                };
            }
        }

        public IEnumerable<Recommendation> BuildRecommendations(QueryAnalysisResult analysisResult)
        {
            yield break;
        }

        private RecommendationCategory MapCategory(ExplainIssueRule rule)
        {
            return rule switch
            {
                ExplainIssueRule.SeqScanOnLargeTable => RecommendationCategory.Indexing,
                ExplainIssueRule.NestedLoopOnLargeTables => RecommendationCategory.Joins,
                ExplainIssueRule.MissingIndex => RecommendationCategory.Indexing,
                ExplainIssueRule.MisestimatedRows => RecommendationCategory.Cardinality,
                ExplainIssueRule.SortWithoutIndex => RecommendationCategory.Sorting,
                ExplainIssueRule.HashAggOnLargeTable => RecommendationCategory.Aggregations,
                ExplainIssueRule.FunctionScan => RecommendationCategory.General,
                ExplainIssueRule.MaterializeNode => RecommendationCategory.ExecutionPlan,
                ExplainIssueRule.UnexpectedParallelism => RecommendationCategory.ExecutionPlan,
                _ => RecommendationCategory.General
            };
        }

        private RecommendationSeverity MapSeverity(AnalysisSeverity severity)
        {
            return severity switch
            {
                AnalysisSeverity.Info => RecommendationSeverity.Info,
                AnalysisSeverity.Warning => RecommendationSeverity.Warning,
                AnalysisSeverity.Critical => RecommendationSeverity.Critical,
                _ => RecommendationSeverity.Info
            };
        }
    }