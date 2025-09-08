namespace SqlAnalyzer.Api.Dto.DbConnection;

/// <summary>
/// Дто для фильтрации существующих соединений в БД
/// </summary>
public class DbConnectionFindDto
{
    /// <summary>
    /// Строка запроса - ищет по названию, хосту, порту, названию БД, юзернейму
    /// </summary>
    public string? Search { get; init; }
    
    /// <summary>
    /// Сколько записей пропускаем
    /// </summary>
    public int? Skip { get; init; }
    
    /// <summary>
    /// Сколько записей берем
    /// </summary>
    public int? Take { get; init; } 
}