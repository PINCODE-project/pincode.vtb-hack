namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

/// <summary>
/// Дто для поиска сохраненных запросов
/// </summary>
public class QueriesFindDto
{
    /// <summary>
    /// Сколько записей пропускаем
    /// </summary>
    public int? Skip { get; init; }
    
    /// <summary>
    /// Сколько записей берем 
    /// </summary>
    public int? Take { get; init; }
}