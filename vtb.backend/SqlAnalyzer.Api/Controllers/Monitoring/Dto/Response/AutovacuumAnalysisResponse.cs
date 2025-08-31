namespace SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

public class AutovacuumAnalysisResponse
{
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
    public List<AutovacuumRecommendation> Recommendations { get; set; } = new();
    public AutovacuumMetricsSummary MetricsSummary { get; set; } = new();
    public string OverallStatus { get; set; } = string.Empty;
    public List<ProblematicTable> ProblematicTables { get; set; } = new();
}

public class AutovacuumMetricsSummary
{
    public int TotalTables { get; set; }
    public int ProblematicTables { get; set; }
    public int CriticalTables { get; set; }
        
    public decimal AvgDeadTupleRatio { get; set; }
        
    public decimal MaxDeadTupleRatio { get; set; }
        
    public string WorstTable { get; set; } = string.Empty;
        
    public decimal WorstTableRatio { get; set; }
        
    public long TotalDeadTuples { get; set; }
    public long TotalLiveTuples { get; set; }
        
    public decimal SystemWideDeadTupleRatio => TotalLiveTuples > 0 ? 
        (decimal)TotalDeadTuples / TotalLiveTuples * 100 : 0;
            
    public int TablesAbove10Percent { get; set; }
    public int TablesAbove20Percent { get; set; }
    public int TablesAbove30Percent { get; set; }
        
    public decimal AvgChangeRatePercent { get; set; }
}

public class ProblematicTable
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long TableSize { get; set; }
    public long LiveTuples { get; set; }
    public long DeadTuples { get; set; }
        
    public decimal DeadTupleRatio { get; set; }
        
    public decimal ChangeRatePercent { get; set; } // % роста dead tuples в час
        
    public DateTime LastAutoVacuum { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string GrowthTrend { get; set; } = string.Empty; // "rapid", "moderate", "slow"
}

public class AutovacuumRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Parameter { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public string RecommendedValue { get; set; } = string.Empty;
    public string SqlCommand { get; set; } = string.Empty;
}
