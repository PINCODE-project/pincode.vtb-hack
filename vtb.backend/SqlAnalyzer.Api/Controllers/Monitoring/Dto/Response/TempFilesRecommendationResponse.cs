namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

// TODO комменты потом
public class TempFilesRecommendationResponse
{
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
    public List<TempFilesRecommendation> Recommendations { get; set; } = new();
    public TempFilesMetricsSummary MetricsSummary { get; set; } = new();
}

public class TempFilesRecommendation
{
    public string Type { get; set; } = string.Empty;
    // TODO энам наверное но в целом пофиг
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high"
    public string Message { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
}

public class TempFilesMetricsSummary
{
    public long TotalTempFiles { get; set; }
    public long TotalTempBytes { get; set; }
    public double TempFilesPerMinute { get; set; }
    public double TempBytesPerMinute { get; set; }
    public double TempBytesPerSecond { get; set; }
}