namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

/// <summary>
/// Связь схема - таблица
/// </summary>
public class SchemaTableDto
{
    /// <summary>
    /// Название схемы
    /// </summary>
    public string SchemaName { get; set; }
    
    /// <summary>
    /// Название таблицы
    /// </summary>
    public string TableName { get; set; }
}