using SqlAnalyzer.Api.Dal.ValueObjects;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.Facade.Interfaces;

public interface ISqlAnalyzerFacade
{
    Task<SqlAlgorithmAnalysisResult> GetAlgorithmResult(string query, ExplainRootPlan? explainResult);
}