namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record QueryDto(Guid Id, string Sql, string ExplainResult, Guid DbConnectionId, DateTime CreatedAt);