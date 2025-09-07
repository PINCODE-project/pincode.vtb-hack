using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.Recommendation.Enums;
using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.Recommendation.Models.Explain;
using SqlAnalyzerLib.Recommendation.Models.Query;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using RecommendationCategory = SqlAnalyzerLib.Recommendation.Enums.RecommendationCategory;

namespace SqlAnalyzerLib.Recommendation;

using Models;
 /// <summary>
    /// Построитель рекомендаций по результатам статического анализа SQL-запроса
    /// </summary>
    public class StaticQueryRecommendationProvider : IRecommendationProvider
    {
        public IEnumerable<Recommendation> BuildRecommendations(QueryAnalysisResult analysisResult)
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

        public IEnumerable<Recommendation> BuildRecommendations(ExplainAnalysisResult analysisResult)
        {
            yield break;
        }

        private RecommendationCategory MapCategory(QueryIssueRule rule)
        {
            return rule switch
            {
                QueryIssueRule.MissingWhereClause => RecommendationCategory.Filtering,
                QueryIssueRule.SelectStarUsage => RecommendationCategory.General,
                QueryIssueRule.CartesianJoin => RecommendationCategory.Joins,
                QueryIssueRule.GroupByWithoutAggregation => RecommendationCategory.Aggregations,
                QueryIssueRule.OrderByWithoutIndex => RecommendationCategory.Sorting,
                QueryIssueRule.LikeWithoutIndex => RecommendationCategory.Filtering,
                QueryIssueRule.NotInUsage => RecommendationCategory.Filtering,
                QueryIssueRule.DistinctWithoutNeed => RecommendationCategory.General,
                QueryIssueRule.FunctionOnIndexedColumn => RecommendationCategory.Indexing,
                QueryIssueRule.ImplicitConversion => RecommendationCategory.General,
                QueryIssueRule.UnnecessarySubquery => RecommendationCategory.Subqueries,
                QueryIssueRule.UnusedCte => RecommendationCategory.General,
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