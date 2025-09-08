namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

/// <summary>
/// Дто для создания SQL запроса
/// </summary>
public class QueryCreateDto
{
    /// <summary>
    /// Id подключения к БД в которой будет выполняться запрос
    /// </summary>
    public required Guid DbConnectionId { get; init; } 
    
    /// <summary>
    /// Содержимое запроса
    /// </summary>
    public required string Sql { get; init; } 
}