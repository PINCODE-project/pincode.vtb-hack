namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionCheckDto(bool IsValid, string? ErrorMessage = null);