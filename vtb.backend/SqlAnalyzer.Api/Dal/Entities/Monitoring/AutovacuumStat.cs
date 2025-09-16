using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

/// <summary>
/// Статистика автовакуума PostgreSQL
/// Содержит метрики для мониторинга и анализа процесса автоматической очистки базы данных
/// Автовакуум удаляет "мертвые" tuple'ы и обновляет статистику для оптимизатора запросов
/// </summary>
public class AutovacuumStat : EntityBase, IEntityCreatedAt
{
    /// <summary>
    /// Дата и время сбора статистики
    /// Используется для построения временных рядов и анализа тенденций
    /// </summary>
    [Required]
    public DateTime CreateAt { get; set; }
        
    /// <summary>
    /// Имя схемы базы данных
    /// Схема представляет собой пространство имен для организации таблиц
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SchemaName { get; set; } = string.Empty;
        
    /// <summary>
    /// Имя таблицы в схеме
    /// Таблица содержит фактические данные и является основной единицей хранения
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;
        
    /// <summary>
    /// Количество "живых" tuple'ов (строк) в таблице
    /// Актуальные данные, доступные для операций SELECT, UPDATE, DELETE
    /// </summary>
    public long LiveTuples { get; set; }
    
    /// <summary>
    /// Количество "мертвых" tuple'ов (строк) в таблице
    /// Удаленные или обновленные строки, которые должны быть очищены автовакуумом
    /// Высокое значение указывает на необходимость vacuum обработки
    /// </summary>
    public long DeadTuples { get; set; }
        
    /// <summary>
    /// Процентное соотношение мертвых tuple'ов к общему количеству
    /// Рассчитывается как: DeadTuples / (LiveTuples + DeadTuples) * 100%
    /// Критическое значение: обычно выше 20-30% требует внимания
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DeadTupleRatio { get; set; }
        
    /// <summary>
    /// Общий размер таблицы в байтах
    /// Включает данные, индексы и служебную информацию
    /// Полезно для мониторинга роста таблиц и планирования ресурсов
    /// </summary>
    public long TableSize { get; set; }
    
    /// <summary>
    /// Дата и время последнего ручного VACUUM
    /// Ручная очистка обычно выполняется администратором для конкретных таблиц
    /// </summary>
    public DateTime? LastVacuum { get; set; }
    
    /// <summary>
    /// Дата и время последнего автоматического VACUUM
    /// Автовакуум запускается PostgreSQL автоматически по расписанию
    /// NULL означает, что автовакуум никогда не выполнялся для этой таблицы
    /// </summary>
    public DateTime? LastAutoVacuum { get; set; }
        
    /// <summary>
    /// Процент изменения размера таблицы за период
    /// Показывает скорость роста/уменьшения таблицы
    /// Положительные значения - рост, отрицательные - уменьшение
    /// Используется для прогнозирования места на диске
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal ChangeRatePercent { get; set; }
    
    /// <summary>
    /// Идентификатор подключения к базе данных
    /// Связывает статистику с конкретным подключением в системе мониторинга
    /// Позволяет различать данные от разных серверов/баз данных
    /// </summary>
    public Guid DbConnectionId { get; set; }
}