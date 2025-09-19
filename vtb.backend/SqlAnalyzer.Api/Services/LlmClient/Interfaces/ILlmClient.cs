using SqlAnalyzer.Api.Dal.ValueObjects;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Services.LlmClient.Interfaces;

public interface ILlmClient
{
    Task<LlmAnswer> GetRecommendation(
        string originalSql,
        ExplainRootPlan? explainJson = null,
        string model = "openai/gpt-oss-120b",
        double temperature = 0.2,
        CancellationToken ct = default);
}