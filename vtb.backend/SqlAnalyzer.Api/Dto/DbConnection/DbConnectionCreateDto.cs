namespace SqlAnalyzer.Api.Dto.DbConnection;

/// <summary>
/// Дто для создания сущности соединения с БД, а также проверки на доступность
/// </summary>
public class DbConnectionCreateDto
{
    /// <summary>
    /// Название БД (внутри сервиса)
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Хост БД
    /// </summary>
    public required string Host { get; init; }
    
    /// <summary>
    /// Порт БД
    /// </summary>
    public required int Port { get; init; }
    
    /// <summary>
    /// Название БД
    /// </summary>
    public required string Database { get; init; }

    /// <summary>
    /// Пользователь под которым логинимся в БД
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Пароль для пользователя в БД
    /// </summary>
    public required string Password { get; init; } 
}