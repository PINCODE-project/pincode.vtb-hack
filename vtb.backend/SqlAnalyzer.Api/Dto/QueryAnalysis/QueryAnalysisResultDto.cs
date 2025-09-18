using SqlAnalyzer.Api.Services.LlmClient.Data;
using SqlAnalyzerLib.Facade.Models;

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
    public required string ExplainResult { get; init; }
    
    /// <summary>
    /// Алгоритмически собранные рекомендации для запроса
    /// </summary>
    public SqlAlgorithmAnalysisResult? AlgorithmRecommendation { get; init; }
    
    /// <summary>
    /// Рекомендации выданные LLM моделью (если использовалась)
    /// </summary>
    public LlmAnswer? LlmRecommendations { get; init; }


    public IReadOnlyCollection<Guid> FindindCustomRules { get; init; } = [];
}