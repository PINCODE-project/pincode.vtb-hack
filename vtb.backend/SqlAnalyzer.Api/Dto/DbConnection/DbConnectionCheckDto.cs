namespace SqlAnalyzer.Api.Dto.DbConnection;

/// <summary>
/// Ответ на проверку доступа к БД
/// </summary>
public class DbConnectionCheckDto
{
    /// <summary>
    /// Доступна ли БД
    /// </summary>
    public required bool IsValid { get; init; }
    
    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; init; }
}