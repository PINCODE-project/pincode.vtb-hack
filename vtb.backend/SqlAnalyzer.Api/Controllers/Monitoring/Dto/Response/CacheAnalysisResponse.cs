namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

/// <summary>
/// Модель ответа для анализа кэша базы данных
/// Содержит результаты анализа эффективности работы кэша PostgreSQL за определенный период
/// </summary>
public class CacheAnalysisResponse
{
    /// <summary>
    /// Начальная дата и время периода анализа
    /// </summary>
    public DateTime AnalysisPeriodStart { get; set; }
    
    /// <summary>
    /// Конечная дата и время периода анализа
    /// </summary>
    public DateTime AnalysisPeriodEnd { get; set; }
    
    /// <summary>
    /// Список рекомендаций по оптимизации кэша
    /// </summary>
    public List<CacheRecommendation> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Сводка метрик кэша за анализируемый период
    /// </summary>
    public CacheMetricsSummary MetricsSummary { get; set; } = new();
    
    /// <summary>
    /// Общий статус здоровья кэша
    /// "healthy" - кэш работает эффективно,
    /// "warning" - требуются некоторые оптимизации,
    /// "critical" - серьезные проблемы с кэшем
    /// </summary>
    public string OverallStatus { get; set; } = string.Empty;
}

/// <summary>
/// Рекомендации специфичные для оптимизации кэша базы данных
/// </summary>
public class CacheRecommendation
{
    /// <summary>
    /// Тип рекомендации по оптимизации кэша
    /// "shared_buffers" - настройка размера shared_buffers,
    /// "index_optimization" - оптимизация индексов,
    /// "query_tuning" - настройка запросов,
    /// "work_mem" - настройка work_mem,
    /// "effective_cache_size" - настройка effective_cache_size
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Уровень серьезности рекомендации
    /// "low" - низкий приоритет,
    /// "medium" - средний приоритет,
    /// "high" - высокий приоритет
    /// </summary>
    public string Severity { get; set; } = string.Empty;
    
    /// <summary>
    /// Детальное описание рекомендации и обоснование
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущее значение параметра (если применимо)
    /// </summary>
    public double CurrentValue { get; set; }
    
    /// <summary>
    /// Рекомендуемое значение параметра
    /// </summary>
    public double RecommendedValue { get; set; }
    
    /// <summary>
    /// Пороговое значение для срабатывания рекомендации
    /// </summary>
    public double Threshold { get; set; }
}

/// <summary>
/// Метрики специфичные для анализа эффективности кэша
/// </summary>
public class CacheMetricsSummary
{
    /// <summary>
    /// Общее количество блоков, найденных в кэше (cache hits)
    /// </summary>
    public long TotalBlksHit { get; set; }
    
    /// <summary>
    /// Общее количество блоков, прочитанных с диска (cache misses)
    /// </summary>
    public long TotalBlksRead { get; set; }
    
    /// <summary>
    /// Общее количество обращений к блокам (hits + reads)
    /// </summary>
    public long TotalBlksAccessed => TotalBlksHit + TotalBlksRead;
        
    /// <summary>
    /// Средний процент попаданий в кэш за период анализа
    /// Рассчитывается как: (TotalBlksHit / TotalBlksAccessed) * 100
    /// </summary>
    public double AvgCacheHitRatio { get; set; }
    
    /// <summary>
    /// Минимальный процент попаданий в кэш за период анализа
    /// </summary>
    public double MinCacheHitRatio { get; set; }
    
    /// <summary>
    /// Максимальный процент попаданий в кэш за период анализа
    /// </summary>
    public double MaxCacheHitRatio { get; set; }
        
    /// <summary>
    /// Количество чтений блоков с диска в минуту
    /// </summary>
    public double BlksReadPerMinute { get; set; }
    
    /// <summary>
    /// Количество попаданий в кэш в минуту
    /// </summary>
    public double BlksHitPerMinute { get; set; }
    
    /// <summary>
    /// Общее количество обращений к блокам в минуту
    /// </summary>
    public double BlksAccessedPerMinute => BlksReadPerMinute + BlksHitPerMinute;
        
    /// <summary>
    /// Соотношение чтений с диска к попаданиям в кэш
    /// Более высокие значения указывают на неэффективность кэша
    /// </summary>
    public double ReadToHitRatio => TotalBlksHit > 0 ? (double)TotalBlksRead / TotalBlksHit : 0;
        
    /// <summary>
    /// Количество точек данных, использованных для анализа
    /// </summary>
    public int DataPointsCount { get; set; }
    
    /// <summary>
    /// Продолжительность периода анализа
    /// </summary>
    public TimeSpan AnalysisDuration { get; set; }
}

/// <summary>
/// Модель для оценки общего здоровья кэша базы данных
/// </summary>
public class CacheHealthStatus
{
    /// <summary>
    /// Общий статус здоровья кэша
    /// "healthy" - кэш работает оптимально (hit ratio > 99%),
    /// "warning" - требуется мониторинг (hit ratio 95-99%),
    /// "critical" - необходимы действия (hit ratio < 95%)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущий процент попаданий в кэш
    /// </summary>
    public double CacheHitRatio { get; set; }
    
    /// <summary>
    /// Сообщение с описанием текущего состояния кэша
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Время оценки состояния кэша
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Список предложений по улучшению эффективности кэша
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}