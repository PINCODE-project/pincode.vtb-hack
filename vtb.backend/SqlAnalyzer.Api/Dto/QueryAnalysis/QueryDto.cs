namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

/// <summary>
/// Дто запроса
/// </summary>
public class QueryDto
{
    /// <summary>
    /// Id запроса
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Содержимое SQL запроса
    /// </summary>
    public required string Sql { get; init; } 
    
    /// <summary>
    /// Резульатат выполнения EXPLAIN для запроса
    /// </summary>
    public required string ExplainResult { get; init; }
    
    /// <summary>
    /// Id подключения к БД в которой выполнялся запрос
    /// </summary>
    public required Guid DbConnectionId { get; init; }
    
    /// <summary>
    /// Дата и время создания
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}