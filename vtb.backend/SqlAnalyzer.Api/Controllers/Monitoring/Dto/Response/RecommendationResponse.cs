namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

// TODO комменты потом
public class RecommendationResponse
{
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
    public List<Recommendation> Recommendations { get; set; } = new();
    public MetricsSummary MetricsSummary { get; set; } = new();
}

public class Recommendation
{
    public string Type { get; set; } = string.Empty;
    // TODO энам наверное но в целом пофиг
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high"
    public string Message { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
}

public class MetricsSummary
{
    public long TotalTempFiles { get; set; }
    public long TotalTempBytes { get; set; }
    public double TempFilesPerMinute { get; set; }
    public double TempBytesPerMinute { get; set; }
    public double TempBytesPerSecond { get; set; }
}