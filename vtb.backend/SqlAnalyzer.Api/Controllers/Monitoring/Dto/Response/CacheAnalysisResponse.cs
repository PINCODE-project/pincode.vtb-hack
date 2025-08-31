namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

/// <summary>
/// Модель ответа для анализа кэша 
/// </summary>
public class CacheAnalysisResponse
{
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
    public List<CacheRecommendation> Recommendations { get; set; } = new();
    public CacheMetricsSummary MetricsSummary { get; set; } = new();
    public string OverallStatus { get; set; } = string.Empty; // "healthy", "warning", "critical"
}

/// <summary>
/// Рекомендации специфичные для кэша
/// </summary>
public class CacheRecommendation
{
    public string Type { get; set; } = string.Empty; // "shared_buffers", "index_optimization", "query_tuning"
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high"
    public string Message { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double RecommendedValue { get; set; }
    public double Threshold { get; set; }
}

// Метрики специфичные для кэша
public class CacheMetricsSummary
{
    public long TotalBlksHit { get; set; }
    public long TotalBlksRead { get; set; }
    public long TotalBlksAccessed => TotalBlksHit + TotalBlksRead;
        
    public double AvgCacheHitRatio { get; set; }
    public double MinCacheHitRatio { get; set; }
    public double MaxCacheHitRatio { get; set; }
        
    public double BlksReadPerMinute { get; set; }
    public double BlksHitPerMinute { get; set; }
    public double BlksAccessedPerMinute => BlksReadPerMinute + BlksHitPerMinute;
        
    public double ReadToHitRatio => TotalBlksHit > 0 ? (double)TotalBlksRead / TotalBlksHit : 0;
        
    public int DataPointsCount { get; set; }
    public TimeSpan AnalysisDuration { get; set; }
}

// Модель для здоровья кэша
public class CacheHealthStatus
{
    public string Status { get; set; } = string.Empty; // "healthy", "warning", "critical"
    public double CacheHitRatio { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<string> Suggestions { get; set; } = new();
}