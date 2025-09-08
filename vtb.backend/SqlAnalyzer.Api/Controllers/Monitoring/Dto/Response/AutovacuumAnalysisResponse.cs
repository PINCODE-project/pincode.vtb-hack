namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

/// <summary>
/// Результаты анализа автовакуума
/// </summary>
public class AutovacuumAnalysisResponse
{
    /// <summary>
    /// Начало анализа
    /// </summary>
    public DateTime AnalysisPeriodStart { get; set; }
    
    /// <summary>
    /// Завершение анализа
    /// </summary>
    public DateTime AnalysisPeriodEnd { get; set; }
    
    /// <summary>
    /// Рекомендации по улучшению вакуума
    /// </summary>
    public List<AutovacuumRecommendation> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Метрики автовакуума
    /// </summary>
    public AutovacuumMetricsSummary MetricsSummary { get; set; } = new();
    
    /// <summary>
    /// Статус автовакуума в целом по всей базе
    /// </summary>
    public string OverallStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Список проблемных таблиц
    /// </summary>
    public List<ProblematicTable> ProblematicTables { get; set; } = new();
}

/// <summary>
/// Сводка метрик автовакуума для мониторинга состояния базы данных
/// </summary>
public class AutovacuumMetricsSummary
{
    /// <summary>
    /// Общее количество таблиц в базе данных
    /// </summary>
    public int TotalTables { get; set; }
    
    /// <summary>
    /// Количество таблиц, требующих внимания (dead tuple ratio > 10%)
    /// </summary>
    public int ProblematicTables { get; set; }
    
    /// <summary>
    /// Количество таблиц в критическом состоянии (dead tuple ratio > 20%)
    /// </summary>
    public int CriticalTables { get; set; }
        
    /// <summary>
    /// Средний процент мертвых строк по всем таблицам
    /// </summary>
    public decimal AvgDeadTupleRatio { get; set; }
        
    /// <summary>
    /// Максимальный процент мертвых строк среди всех таблиц
    /// </summary>
    public decimal MaxDeadTupleRatio { get; set; }
        
    /// <summary>
    /// Наименование таблицы с наихудшим показателем мертвых строк
    /// </summary>
    public string WorstTable { get; set; } = string.Empty;
        
    /// <summary>
    /// Процент мертвых строк в таблице с наихудшим показателем
    /// </summary>
    public decimal WorstTableRatio { get; set; }
        
    /// <summary>
    /// Общее количество мертвых строк во всех таблицах
    /// </summary>
    public long TotalDeadTuples { get; set; }
    
    /// <summary>
    /// Общее количество живых строк во всех таблицах
    /// </summary>
    public long TotalLiveTuples { get; set; }
        
    /// <summary>
    /// Общий процент мертвых строк (dead tuples) во всех базах данных кластера
    /// Рассчитывается как: (TotalDeadTuples / TotalLiveTuples) * 100
    /// </summary>
    public decimal SystemWideDeadTupleRatio => TotalLiveTuples > 0 ? 
        (decimal)TotalDeadTuples / TotalLiveTuples * 100 : 0;
     
    /// <summary>
    /// Количество таблиц с процентом мертвых строк более 10%
    /// </summary>
    public int TablesAbove10Percent { get; set; }
    
    /// <summary>
    /// Количество таблиц с процентом мертвых строк более 20%
    /// </summary>
    public int TablesAbove20Percent { get; set; }
    
    /// <summary>
    /// Количество таблиц с процентом мертвых строк более 30%
    /// </summary>
    public int TablesAbove30Percent { get; set; }
        
    /// <summary>
    /// Средний процент изменения данных в таблицах (скорость накопления мертвых строк)
    /// </summary>
    public decimal AvgChangeRatePercent { get; set; }
}

/// <summary>
/// Информация о проблемной таблице, требующей внимания автовакуума
/// </summary>
public class ProblematicTable
{
    /// <summary>
    /// Наименование схемы базы данных, в которой находится таблица
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;
    
    /// <summary>
    /// Наименование проблемной таблицы
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    
    /// <summary>
    /// Размер таблицы в байтах
    /// </summary>
    public long TableSize { get; set; }
    
    /// <summary>
    /// Количество живых (актуальных) строк в таблице
    /// </summary>
    public long LiveTuples { get; set; }
    
    /// <summary>
    /// Количество мертвых строк (удаленных или устаревших версий) в таблице
    /// </summary>
    public long DeadTuples { get; set; }
        
    /// <summary>
    /// Процентное соотношение мертвых строк к общему количеству строк
    /// Рассчитывается как: (DeadTuples / (LiveTuples + DeadTuples)) * 100
    /// </summary>
    public decimal DeadTupleRatio { get; set; }
        
    /// <summary>
    /// Процент роста мертвых строк в час
    /// Показывает скорость накопления мертвых данных
    /// </summary>
    public decimal ChangeRatePercent { get; set; }
        
    /// <summary>
    /// Время последнего запуска автовакуума для данной таблицы
    /// </summary>
    public DateTime LastAutoVacuum { get; set; }
    
    /// <summary>
    /// Приоритет обработки таблицы ("high", "medium", "low")
    /// Определяется на основе DeadTupleRatio и ChangeRatePercent
    /// </summary>
    public string Priority { get; set; } = string.Empty;
    
    /// <summary>
    /// Тенденция роста мертвых строк ("rapid" - быстрый, "moderate" - умеренный, "slow" - медленный)
    /// </summary>
    public string GrowthTrend { get; set; } = string.Empty;
}

/// <summary>
/// Рекомендация по настройке автовакуума для оптимизации работы базы данных
/// </summary>
public class AutovacuumRecommendation
{
    /// <summary>
    /// Тип рекомендации ("vacuum", "analyze", "parameter_tuning", "maintenance")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Уровень серьезности проблемы ("critical", "warning", "info")
    /// </summary>
    public string Severity { get; set; } = string.Empty;
    
    /// <summary>
    /// Текстовое описание рекомендации или проблемы
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя таблицы, к которой относится рекомендация
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    
    /// <summary>
    /// Наименование параметра PostgreSQL, который требует настройки
    /// </summary>
    public string Parameter { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущее значение параметра
    /// </summary>
    public string CurrentValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Рекомендуемое значение параметра
    /// </summary>
    public string RecommendedValue { get; set; } = string.Empty;
    
    /// <summary>
    /// SQL-команда для применения рекомендации
    /// </summary>
    public string SqlCommand { get; set; } = string.Empty;
}
