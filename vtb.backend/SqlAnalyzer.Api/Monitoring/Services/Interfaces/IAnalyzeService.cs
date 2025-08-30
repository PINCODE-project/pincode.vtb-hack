using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

/// <summary>
/// Сервис для анализа метрик бд
/// </summary>
public interface IAnalyzeService
{
    /// <summary>
    /// Анализ состояния бд
    /// </summary>
    /// <returns>Рекомендации по оптимизации бд</returns>
    Task<RecommendationResponse> AnalyzeTempFilesLastHourAsync();
}