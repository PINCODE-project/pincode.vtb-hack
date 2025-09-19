using SqlAnalyzer.Api.Dal.ValueObjects;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Dto.QueryAnalysis;

/// <summary>
/// Результат анализа SQL запроса
/// </summary>
public class QueryAnalysisResultDto
{
    /// <summary>
    /// Id запроса
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Id подключения к БД
    /// </summary>
    public required Guid DbConnectionId { get; init; }
    
    /// <summary>
    /// Содержимое SQL запроса
    /// </summary>
    public required string Query { get; init; }
    
    /// <summary>
    /// Результат выполнения EXPLAIN для запроса
    /// </summary>
    public required ExplainRootPlan? ExplainResult { get; init; }
    
    /// <summary>
    /// Алгоритмически собранные рекомендации для запроса
    /// </summary>
    public SqlAlgorithmAnalysisResult? AlgorithmRecommendation { get; init; }
    
    /// <summary>
    /// Рекомендации выданные LLM моделью (если использовалась)
    /// </summary>
    public SqlLlmAnalysisResult? LlmRecommendations { get; init; }
    
    /// <summary>
    /// Айдишники найденных кастомных правил
    /// </summary>
    public IReadOnlyCollection<Guid> FindindCustomRules { get; init; } = [];
    
    /// <summary>
    /// Результат сравнения EXPLAIN для входного запроса и запроса с LLM
    /// </summary>
    public PlanComparisonDto? ExplainComparisonDto { get; init; }
}