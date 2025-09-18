using SqlAnalyzer.Api.Dal.ValueObjects;

namespace SqlAnalyzer.Api.Services.LlmClient.Interfaces;

public interface ILlmClient
{
    Task<LlmAnswer> GetRecommendation(
        string originalSql,
        string? explainJson = null,
        string model = "openai/gpt-oss-120b",
        double temperature = 0.2,
        CancellationToken ct = default);
}