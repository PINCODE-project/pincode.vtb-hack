using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.Facade.Interfaces;
using SqlAnalyzerLib.Facade.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.Facade;

public class SqlAnalyzerFacade : ISqlAnalyzerFacade
{
    private readonly IStaticSqlAnalyzer _staticSqlAnalyzer;
    private readonly IExplainAnalyzer _explainAnalyzer;

    public SqlAnalyzerFacade(IStaticSqlAnalyzer staticSqlAnalyzer, IExplainAnalyzer ruleEngine)
    {
        _staticSqlAnalyzer = staticSqlAnalyzer;
        _explainAnalyzer = ruleEngine;
    }

    /// <inheritdoc />
    public async Task<SqlAlgorithmAnalysisResult> GetRecommendations(string query, string explainResult)
    {
        var staticAnalysisResult = await _staticSqlAnalyzer.AnalyzeAsync(new SqlQuery(query));
        ExplainAnalysisResult? explainAnalysisResult = null;
        if (!string.IsNullOrEmpty(explainResult))
        {
            explainAnalysisResult = await _explainAnalyzer.AnalyzeAsync(query, explainResult);
        }

        return new SqlAlgorithmAnalysisResult
        {
            QueryAnalysisResult = staticAnalysisResult,
            ExplainAnalysisResult = explainAnalysisResult
        };
    }
}