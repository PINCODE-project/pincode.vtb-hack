using SqlAnalyzer.Api.Dal.ValueObjects;

namespace SqlAnalyzerLib.Facade.Interfaces;

public interface ISqlAnalyzerFacade
{
    Task<SqlAlgorithmAnalysisResult> GetRecommendations(string query, string explainResult);
}