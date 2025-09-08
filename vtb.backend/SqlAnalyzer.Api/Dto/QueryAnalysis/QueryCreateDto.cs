namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record QueryCreateDto(Guid DbConnectionId, string Sql);