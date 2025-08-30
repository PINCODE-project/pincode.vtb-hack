namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionCreateDto(
    string Host,
    int Port,
    string Database,
    string Username,
    string Password
);