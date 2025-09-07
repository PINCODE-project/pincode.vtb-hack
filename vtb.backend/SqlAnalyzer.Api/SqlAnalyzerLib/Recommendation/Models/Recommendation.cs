using SqlAnalyzerLib.Recommendation.Enums;

namespace SqlAnalyzerLib.Recommendation.Models;

/// <summary>
/// Модель рекомендации по оптимизации
/// </summary>
public class Recommendation
{
    public RecommendationCategory Category { get; set; }
    public RecommendationSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
}