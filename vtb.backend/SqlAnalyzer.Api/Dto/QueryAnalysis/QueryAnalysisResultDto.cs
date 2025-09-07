using SqlAnalyzer.Api.Services.LlmClient.Data;

namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record QueryAnalysisResultDto(
    Guid Id,
    Guid DbConnectionId,
    string Query,
    string ExplainResult,
    string AlgorithmRecommendation,
    LlmAnswer LlmRecommendations
);