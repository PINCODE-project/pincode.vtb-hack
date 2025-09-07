namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionDto(
    Guid Id,
    string Name,
    string Host,
    int Port,
    string Database,
    string Username
);