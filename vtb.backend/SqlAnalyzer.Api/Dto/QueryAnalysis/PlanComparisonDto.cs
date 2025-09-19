namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public class PlanComparisonDto
{
    public PlanPointComparsionResult Cost { get; init; }

    public PlanPointComparsionResult Rows { get; init; }
    
    public PlanPointComparsionResult Width { get; init; }

    public PlanPointComparsionResult SeqScanCount { get; init; }
    
    public PlanPointComparsionResult NodeCount { get; init;}

    public string OldJoinTypes { get; set; } = string.Empty;
    public string NewJoinTypes { get; set; } = string.Empty;
}