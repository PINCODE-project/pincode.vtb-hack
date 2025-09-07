namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionUpdateDto(
    Guid Id,
    string? Name,
    string? Host,
    int? Port,
    string? Database,
    string? Username,
    string? Password
    );