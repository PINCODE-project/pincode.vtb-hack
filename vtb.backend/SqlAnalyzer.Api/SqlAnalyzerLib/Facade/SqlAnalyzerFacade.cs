using SqlAnalyzerLib.ExplainAnalysis;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.Facade.Interfaces;
using SqlAnalyzerLib.Facade.Mappers;
using SqlAnalyzerLib.Recommendation;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.Facade;

using Recommendation.Models;
public class SqlAnalyzerFacade : ISqlAnalyzerFacade
{
    private readonly IStaticSqlAnalyzer _staticSqlAnalyzer;
    private readonly IRuleEngine _ruleEngine;
    private readonly RecommendationEngine _recommendationEngine;

    public SqlAnalyzerFacade(
        IStaticSqlAnalyzer staticSqlAnalyzer,
        IRuleEngine ruleEngine,
        RecommendationEngine recommendationEngine
    )
    {
        _staticSqlAnalyzer = staticSqlAnalyzer;
        _ruleEngine = ruleEngine;
        _recommendationEngine = recommendationEngine;
    }

    public async Task<IReadOnlyCollection<Recommendation>> GetRecommendations(string query, string explainResult)
    {
        var staticResult = await _staticSqlAnalyzer.AnalyzeAsync(new SqlQuery(query));
        var queryAnalysisResult = staticResult.ToQueryAnalysisResult();
        
        var parser = new ExplainJsonParser();
        ExplainRootPlan rootPlan;
        try
        {
            rootPlan = parser.Parse(explainResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка парсинга EXPLAIN JSON: " + ex.Message);
            return Array.Empty<Recommendation>();
        }
        
        var findings = await _ruleEngine.EvaluateAllAsync(rootPlan);
        var explainAnalysisResult = findings.ToExplainAnalysisResult(query);
        
        var recommendations = _recommendationEngine.BuildRecommendations(queryAnalysisResult, explainAnalysisResult);
        return recommendations;
    }
}