namespace SqlAnalyzer.Api.Dto.DbConnection;

/// <summary>
/// Дто для обновления сущности соединения с БД
/// </summary>
public record DbConnectionUpdateDto
{
    /// <summary>
    /// Id 
    /// </summary>
    public required  Guid Id { get; init; } 
    
    /// <summary>
    /// Название БД (внутри сервиса)
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// Хост БД
    /// </summary>
    public string? Host { get; init; }
    
    /// <summary>
    /// Порт БД
    /// </summary>
    public int? Port { get; init; }
    
    /// <summary>
    /// Название БД
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// Пользователь под которым логинимся в БД
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Пароль для пользователя в БД
    /// </summary>
    public string? Password { get; init; } 
}