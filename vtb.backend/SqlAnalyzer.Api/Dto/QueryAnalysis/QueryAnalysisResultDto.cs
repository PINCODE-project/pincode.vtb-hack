namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record QueryAnalysisResultDto(
    Guid Id,
    Guid DbConnectionId,
    string Query,
    string AnalyzeResult,
    DateTime CreatedAt
);