namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionCreateDto(
    string Name,
    string Host,
    int Port,
    string Database,
    string Username,
    string Password
);