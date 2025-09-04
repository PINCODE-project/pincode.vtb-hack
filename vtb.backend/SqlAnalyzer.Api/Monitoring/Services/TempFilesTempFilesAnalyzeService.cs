using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

/// <inheritdoc />
internal class TempFilesTempFilesAnalyzeService : ITempFilesAnalyzeService
{
    private readonly ILogger<TempFilesTempFilesAnalyzeService> _logger;
    private readonly DataContext _db;

    public TempFilesTempFilesAnalyzeService(ILogger<TempFilesTempFilesAnalyzeService> logger,
        DataContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<TempFilesRecommendationResponse> AnalyzeTempFilesLastHourAsync()
    {
        var response = new TempFilesRecommendationResponse
        {
            AnalysisPeriodEnd = DateTime.UtcNow,
            AnalysisPeriodStart = DateTime.UtcNow.AddDays(-1)
        };

        try
        {
            // Получаем статистику за последний час
            var stats = await GetStatsForPeriodAsync(response.AnalysisPeriodStart, response.AnalysisPeriodEnd);
                    
            if (stats.Count < 2)
            {
                response.Recommendations.Add(new TempFilesRecommendation
                {
                    Type = "data",
                    Severity = "low",
                    Message = "Недостаточно данных для анализа. Требуется как минимум 2 замера за период.",
                    CurrentValue = stats.Count,
                    Threshold = 2
                });
                return response;
            }

            // Анализируем метрики
            response.MetricsSummary = CalculateMetricsSummary(stats);
            response.Recommendations = GenerateRecommendations(response.MetricsSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе данных");
            response.Recommendations.Add(new TempFilesRecommendation
            {
                Type = "system",
                Severity = "high",
                Message = $"Ошибка анализа: {ex.Message}",
                CurrentValue = 0,
                Threshold = 0
            });
        }

        return response;
    }

    private async Task<List<TempFilesStatsDal>> GetStatsForPeriodAsync(DateTime start, DateTime end)
    {
        var tempFilesStatList = await _db.TempFilesStats.Where(x => x.CreateAt >= start && x.CreateAt <= end).ToListAsync();
        return tempFilesStatList;
    }

    private static TempFilesMetricsSummary CalculateMetricsSummary(List<TempFilesStatsDal> stats)
    {
        if (stats.Count < 2)
        {
            return new TempFilesMetricsSummary();
        }

        var first = stats.First();
        var last = stats.Last();

        var minutesInPeriod = (last.CreateAt - first.CreateAt).TotalMinutes;
        if (minutesInPeriod == 0)
        {
            minutesInPeriod = 1;
        }

        var tempFilesGrowth = last.TempFiles - first.TempFiles;
        var tempBytesGrowth = last.TempBytes - first.TempBytes;

        return new TempFilesMetricsSummary
        {
            TotalTempFiles = tempFilesGrowth,
            TotalTempBytes = tempBytesGrowth,
            TempFilesPerMinute = tempFilesGrowth / minutesInPeriod,
            TempBytesPerMinute = tempBytesGrowth / minutesInPeriod,
            TempBytesPerSecond = tempBytesGrowth / (minutesInPeriod * 60)
        };
    }

    private static List<TempFilesRecommendation> GenerateRecommendations( 
        TempFilesMetricsSummary summary)
    {
        var recommendations = new List<TempFilesRecommendation>();

        // Анализ временных файлов
        if (summary.TempFilesPerMinute > 2) // > 2 файлов в минуту
        {
            recommendations.Add(new TempFilesRecommendation
            {
                Type = "work_mem",
                Severity = summary.TempFilesPerMinute > 10 ? "high" : "medium",
                Message = $"Высокий рост временных файлов: {summary.TempFilesPerMinute:F2} файлов/мин. " +
                          $"Это указывает на нехватку памяти для операций сортировки и агрегации.",
                CurrentValue = summary.TempFilesPerMinute,
                Threshold = 2
            });
        }

        if (summary.TempBytesPerSecond > 1024 * 1024) // > 1 MB/s
        {
            recommendations.Add(new TempFilesRecommendation
            {
                Type = "work_mem_critical",
                Severity = "high",
                Message = $"КРИТИЧЕСКИЙ рост временных файлов: {summary.TempBytesPerSecond / (1024 * 1024):F2} MB/сек. " +
                          $"Серьезная нехватка work_mem, необходимо немедленное вмешательство.",
                CurrentValue = summary.TempBytesPerSecond,
                Threshold = 1024 * 1024 // 1 MB
            });
        }

        // Если проблем не обнаружено
        if (recommendations.All(r => r.Severity != "high"))
        {
            recommendations.Add(new TempFilesRecommendation
            {
                Type = "health",
                Severity = "low",
                Message = "Система работает стабильно. Значения временных файлов в пределах нормы.",
                CurrentValue = summary.TempFilesPerMinute,
                Threshold = 2
            });
        }

        return recommendations;
    }
}