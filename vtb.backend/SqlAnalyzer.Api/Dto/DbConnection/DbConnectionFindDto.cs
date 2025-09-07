namespace SqlAnalyzer.Api.Dto.DbConnection;

public record DbConnectionFindDto(
    string? Search,
    int? Skip,
    int? Take
    );