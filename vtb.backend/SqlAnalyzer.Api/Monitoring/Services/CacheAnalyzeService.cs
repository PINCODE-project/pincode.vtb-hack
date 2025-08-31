using Npgsql;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

internal class CacheAnalyzeService : ICacheAnalyzeService
{
    private readonly ILogger<CacheAnalyzeService> _logger;
    private readonly string _monitoringConnectionString;

    public CacheAnalyzeService(ILogger<CacheAnalyzeService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _monitoringConnectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<CacheAnalysisResponse> AnalyzeCacheLastHourAsync()
    {
        var response = new CacheAnalysisResponse
        {
            AnalysisPeriodEnd = DateTime.UtcNow,
            AnalysisPeriodStart = DateTime.UtcNow.AddHours(-1)
        };

        try
        {
            await using var connection = new NpgsqlConnection(_monitoringConnectionString);
            await connection.OpenAsync();
                    
            var stats = await GetCacheStatsForPeriodAsync(connection, response.AnalysisPeriodStart, response.AnalysisPeriodEnd);
                    
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

    public async Task<CacheHealthStatus> GetCacheHealthStatusAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_monitoringConnectionString);
            await connection.OpenAsync();
                    
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddHours(-1);
            var stats = await GetCacheStatsForPeriodAsync(connection, startTime, endTime);

            if (stats.Count == 0)
            {
                return new CacheHealthStatus
                {
                    Status = "unknown",
                    CacheHitRatio = 0,
                    Message = "Данные отсутствуют",
                    Timestamp = DateTime.UtcNow
                };
            }

            var metrics = CalculateCacheMetricsSummary(stats);
            var status = DetermineOverallStatus(metrics.AvgCacheHitRatio);

            return new CacheHealthStatus
            {
                Status = status,
                CacheHitRatio = metrics.AvgCacheHitRatio,
                Message = GetHealthStatusMessage(status, metrics.AvgCacheHitRatio),
                Timestamp = DateTime.UtcNow,
                Suggestions = GetHealthSuggestions(status, metrics)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса здоровья кэша");
            return new CacheHealthStatus
            {
                Status = "error",
                CacheHitRatio = 0,
                Message = $"Ошибка: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private async Task<List<CacheStatsData>> GetCacheStatsForPeriodAsync(NpgsqlConnection connection, DateTime start, DateTime end)
    {
        var stats = new List<CacheStatsData>();
        var query = @"
                SELECT measurement_time, blks_hit, blks_read, cache_hit_ratio 
                FROM cache_hit_stats 
                WHERE measurement_time BETWEEN @start AND @end
                ORDER BY measurement_time";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            stats.Add(new CacheStatsData
            {
                MeasurementTime = reader.GetDateTime(0),
                BlksHit = reader.GetInt64(1),
                BlksRead = reader.GetInt64(2),
                CacheHitRatio = reader.GetDecimal(3)
            });
        }

        return stats;
    }

    private CacheMetricsSummary CalculateCacheMetricsSummary(List<CacheStatsData> stats)
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
            var duration = last.MeasurementTime - first.MeasurementTime;

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
        if (metrics.AvgCacheHitRatio < 99.0)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "memory_optimization",
                Severity = metrics.AvgCacheHitRatio < 90.0 ? "high" : "medium",
                Message = $"Cache hit ratio ниже оптимального: {metrics.AvgCacheHitRatio:F2}%",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 99.0,
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
        if (metrics.AvgCacheHitRatio < 95.0 && metrics.BlksReadPerMinute > 2000)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "shared_buffers",
                Severity = "high",
                Message = "Критическая нехватка памяти. Рекомендуется увеличить shared_buffers",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 99.0,
                Threshold = 95.0
            });
        }

        // Если все хорошо
        if (metrics.AvgCacheHitRatio >= 99.5 && metrics.BlksReadPerMinute < 100)
        {
            recommendations.Add(new CacheRecommendation
            {
                Type = "performance",
                Severity = "low",
                Message = "Отличная производительность кэша!",
                CurrentValue = metrics.AvgCacheHitRatio,
                RecommendedValue = 99.5,
                Threshold = 99.0
            });
        }

        return recommendations;
    }

    private string DetermineOverallStatus(double cacheHitRatio)
    {
        return cacheHitRatio switch
        {
            >= 99.0 => "healthy",
            >= 95.0 => "warning",
            _ => "critical"
        };
    }

    private string GetHealthStatusMessage(string status, double cacheHitRatio)
    {
        return status switch
        {
            "healthy" => $"Кэш работает отлично: {cacheHitRatio:F2}%",
            "warning" => $"Требуется внимание: {cacheHitRatio:F2}%",
            "critical" => $"Критическое состояние: {cacheHitRatio:F2}%",
            _ => "Неизвестный статус"
        };
    }

    private List<string> GetHealthSuggestions(string status, CacheMetricsSummary metrics)
    {
        var suggestions = new List<string>();

        if (status == "critical")
        {
            suggestions.Add("Увеличьте shared_buffers (25-40% от общей памяти)");
            suggestions.Add("Проанализируйте запросы с последовательным сканированием");
            suggestions.Add("Добавьте индексы для часто используемых запросов");
        }
        else if (status == "warning")
        {
            suggestions.Add("Рассмотрите увеличение shared_buffers");
            suggestions.Add("Оптимизируйте самые частые запросы");
            suggestions.Add("Добавьте индексы для медленных запросов");
        }
        else
        {
            suggestions.Add("Продолжайте текущую конфигурацию");
            suggestions.Add("Регулярно мониторьте производительность");
        }

        return suggestions;
    }
}