using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

internal class CacheAnalyzeService : ICacheAnalyzeService
{
    private readonly ILogger<CacheAnalyzeService> _logger;
    private readonly DataContext _db;

    public CacheAnalyzeService(ILogger<CacheAnalyzeService> logger,
        DataContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<CacheAnalysisResponse> AnalyzeCacheAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd)
    {
        var response = new CacheAnalysisResponse
        {
            AnalysisPeriodEnd = periodEnd,
            AnalysisPeriodStart = periodStart,
        };

        try
        {
            var stats = await GetCacheStatsForPeriodAsync(dbConnectionId, response.AnalysisPeriodStart, response.AnalysisPeriodEnd);
                    
            if (stats.Count < 2)
            {
                response.Recommendations.Add(new CacheRecommendation
                {
                    Type = "data_availability",
                    Severity = "low",
                    Message = "Недостаточно данных для анализа. Соберите больше данных за период.",
                    CurrentValue = stats.Count,
                    RecommendedValue = 12, // 12 записей за час при сборе каждые 5 минут
                    Threshold = 2
                });
                return response;
            }

            response.MetricsSummary = CalculateCacheMetricsSummary(stats);
            response.Recommendations = GenerateCacheRecommendations(response.MetricsSummary);
            response.OverallStatus = DetermineOverallStatus(response.MetricsSummary.AvgCacheHitRatio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе данных кэша");
            response.Recommendations.Add(new CacheRecommendation
            {
                Type = "system_error",
                Severity = "high",
                Message = $"Ошибка анализа: {ex.Message}",
                CurrentValue = 0,
                RecommendedValue = 0,
                Threshold = 0
            });
            response.OverallStatus = "error";
        }

        return response;
    }

    private async Task<List<CacheHitStats>> GetCacheStatsForPeriodAsync(Guid dbConnectionId, DateTime start, DateTime end)
    {
        if (end == DateTime.MinValue)
        {
            var cacheStatsForAllTimeList = await _db.CacheHitStats
                .Where(x => x.CreateAt >= start && x.DbConnectionId == dbConnectionId)
                .ToListAsync();
            return cacheStatsForAllTimeList;
        }
        var cacheStatsList = await _db.CacheHitStats
            .Where(x => x.CreateAt >= start && x.CreateAt <= end && x.DbConnectionId == dbConnectionId)
            .ToListAsync();
        return cacheStatsList;
    }

    private CacheMetricsSummary CalculateCacheMetricsSummary(List<CacheHitStats> stats)
    {
        var summary = new CacheMetricsSummary
        {
            DataPointsCount = stats.Count,
            MinCacheHitRatio = stats.Count > 0 ? (double)stats.Min(s => s.CacheHitRatio) : 0,
            MaxCacheHitRatio = stats.Count > 0 ? (double)stats.Max(s => s.CacheHitRatio) : 0,
            AvgCacheHitRatio = stats.Count > 0 ? (double)stats.Average(s => s.CacheHitRatio) : 0
        };

        if (stats.Count >= 2)
        {
            var first = stats.First();
            var last = stats.Last();
            var duration = last.CreateAt - first.CreateAt;

            summary.TotalBlksHit = last.BlksHit - first.BlksHit;
            summary.TotalBlksRead = last.BlksRead - first.BlksRead;
            summary.AnalysisDuration = duration;

            if (duration.TotalMinutes > 0)
            {
                summary.BlksHitPerMinute = summary.TotalBlksHit / duration.TotalMinutes;
                summary.BlksReadPerMinute = summary.TotalBlksRead / duration.TotalMinutes;
            }
        }

        return summary;
    }

    private List<CacheRecommendation> GenerateCacheRecommendations(CacheMetricsSummary metrics)
    {
        var recommendations = new List<CacheRecommendation>();

        // Рекомендации по cache hit ratio
        if (metrics.AvgCacheHitRatio < 90.0)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "memory_optimization",
                Severity = metrics.AvgCacheHitRatio < 90.0 ? "high" : "medium",
                Message = $"Cache hit ratio ниже оптимального: {metrics.AvgCacheHitRatio:F2}%",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 90.0,
                Threshold = 95.0
            });
        }

        // Рекомендации по чтению с диска
        if (metrics.BlksReadPerMinute > 1000)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "io_optimization",
                Severity = metrics.BlksReadPerMinute > 5000 ? "high" : "medium",
                Message = $"Высокая дисковая активность: {metrics.BlksReadPerMinute:F0} чтений/мин",
                CurrentValue = metrics.BlksReadPerMinute,
                RecommendedValue = 500.0,
                Threshold = 1000.0
            });
        }

        // Рекомендация по увеличению shared_buffers
        if (metrics.AvgCacheHitRatio < 85.0 && metrics.BlksReadPerMinute > 2000)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "shared_buffers",
                Severity = "high",
                Message = "Критическая нехватка памяти. Рекомендуется увеличить shared_buffers",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 90.0,
                Threshold = 95.0
            });
        }

        // Если все хорошо
        if (metrics.AvgCacheHitRatio >= 90.0 && metrics.BlksReadPerMinute < 100)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "performance",
                Severity = "low",
                Message = "Отличная производительность кэша!",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 90.0,
                Threshold = 99.0
            });
        }

        return recommendations;
    }

    private string DetermineOverallStatus(double cacheHitRatio)
    {
        return cacheHitRatio switch
        {
            >= 90.0 => "healthy",
            >= 85.0 => "warning",
            _ => "critical"
        };
    }
}