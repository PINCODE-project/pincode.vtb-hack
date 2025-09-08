namespace SqlAnalyzer.Api.Dto.DbConnection;

/// <summary>
/// 
/// </summary>
public class DbConnectionDto
{
    /// <summary>
    /// Id 
    /// </summary>
    public required  Guid Id { get; init; } 
    
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
}