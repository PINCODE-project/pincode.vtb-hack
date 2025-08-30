namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record QueryAnalysisDto(Guid DbConnectionId, string Sql);