namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

public record PlanPointComparsionResult(decimal Old, decimal New, decimal? DifferencePercent);
