using SqlAnalyzer.Api.Dal.ValueObjects;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.Facade.Interfaces;
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
    public async Task<SqlAlgorithmAnalysisResult> GetAlgorithmResult(string query, ExplainRootPlan? explainResult)
    {
        var staticAnalysisResult = await _staticSqlAnalyzer.AnalyzeAsync(new SqlQuery(query));
        var explainAnalysisResult = explainResult is not null
            ? await _explainAnalyzer.AnalyzeAsync(query, explainResult)
            : null;

        return new SqlAlgorithmAnalysisResult
        {
            QueryAnalysisResult = staticAnalysisResult,
            ExplainAnalysisResult = explainAnalysisResult
        };
    }
}