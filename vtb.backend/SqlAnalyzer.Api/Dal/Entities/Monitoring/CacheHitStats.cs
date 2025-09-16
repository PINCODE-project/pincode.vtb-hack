using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

/// <summary>
/// Статистика попаданий в кэш PostgreSQL
/// Отслеживает эффективность использования кэша базы данных
/// </summary>
public class CacheHitStats : EntityBase, IEntityCreatedAt
{
    /// <summary>
    /// Количество блоков, прочитанных из кэша (буферного кэша PostgreSQL)
    /// Блоки, которые были найдены в shared_buffers без необходимости дискового I/O
    /// Высокое значение указывает на хорошую производительность кэширования
    /// </summary>
    public long BlksHit { get; set; }
    
    /// <summary>
    /// Количество блоков, прочитанных с диска
    /// Блоки, которые не были найдены в кэше и потребовали физического чтения с диска
    /// Высокое значение может указывать на нехватку оперативной памяти или неэффективные запросы
    /// </summary>
    public long BlksRead { get; set; }
    
    /// <summary>
    /// Коэффициент попадания в кэш (Cache Hit Ratio)
    /// Рассчитывается как: BlksHit / (BlksHit + BlksRead) * 100%
    /// Показывает процент запросов, которые были обслужены из кэша
    /// Оптимальное значение: выше 99% для production-систем
    /// Значение ниже 90% может указывать на проблемы с производительностью
    /// </summary>
    public decimal CacheHitRatio { get; set; }
    
    /// <summary>
    /// Дата и время сбора статистики
    /// Позволяет отслеживать изменения эффективности кэширования во времени
    /// </summary>
    public DateTime CreateAt { get; set; }
    
    /// <summary>
    /// Идентификатор подключения к базе данных
    /// Связывает статистику с конкретным подключением/базой данных в системе мониторинга
    /// </summary>
    public Guid DbConnectionId { get; set; }
}