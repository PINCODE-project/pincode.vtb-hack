using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

/// <summary>
/// Сервис для анализа метрик бд
/// </summary>
public interface ITempFilesAnalyzeService
{
    /// <summary>
    /// Анализ состояния бд
    /// </summary>
    /// <returns>Рекомендации по оптимизации бд</returns>
    Task<TempFilesRecommendationResponse> AnalyzeTempFilesAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd);
}