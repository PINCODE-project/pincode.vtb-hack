using Npgsql;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

/// <inheritdoc />
internal class AnalyzeService : IAnalyzeService
{
    private readonly ILogger<AnalyzeService> _logger;
    private readonly string _monitoringConnectionString;

    public AnalyzeService(ILogger<AnalyzeService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _monitoringConnectionString = configuration.GetConnectionString("MonitoringDatabase");
    }

    public async Task<RecommendationResponse> AnalyzeTempFilesLastHourAsync()
    {
        var response = new RecommendationResponse
        {
            AnalysisPeriodEnd = DateTime.UtcNow,
            AnalysisPeriodStart = DateTime.UtcNow.AddHours(-1)
        };

        try
        {
            await using var connection = new NpgsqlConnection(_monitoringConnectionString);
            await connection.OpenAsync();
                    
            // Получаем статистику за последний час
            var stats = await GetStatsForPeriodAsync(connection, response.AnalysisPeriodStart, response.AnalysisPeriodEnd);
                    
            if (stats.Count < 2)
            {
                response.Recommendations.Add(new Recommendation
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
            response.Recommendations.Add(new Recommendation
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

    private async Task<List<TempFilesStats>> GetStatsForPeriodAsync(NpgsqlConnection connection, DateTime start, DateTime end)
    {
        var stats = new List<TempFilesStats>();
        var query = @"
                SELECT measurement_time, temp_files, temp_bytes 
                FROM temp_files_stats 
                WHERE measurement_time BETWEEN @start AND @end
                ORDER BY measurement_time";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            stats.Add(new TempFilesStats
            {
                MeasurementTime = reader.GetDateTime(0),
                TempFiles = reader.GetInt64(1),
                TempBytes = reader.GetInt64(2)
            });
        }

        return stats;
    }

    private static MetricsSummary CalculateMetricsSummary(List<TempFilesStats> stats)
    {
        if (stats.Count < 2) return new MetricsSummary();

        var first = stats.First();
        var last = stats.Last();

        var minutesInPeriod = (last.MeasurementTime - first.MeasurementTime).TotalMinutes;
        if (minutesInPeriod == 0) minutesInPeriod = 1;

        var tempFilesGrowth = last.TempFiles - first.TempFiles;
        var tempBytesGrowth = last.TempBytes - first.TempBytes;

        return new MetricsSummary
        {
            TotalTempFiles = tempFilesGrowth,
            TotalTempBytes = tempBytesGrowth,
            TempFilesPerMinute = tempFilesGrowth / minutesInPeriod,
            TempBytesPerMinute = tempBytesGrowth / minutesInPeriod,
            TempBytesPerSecond = tempBytesGrowth / (minutesInPeriod * 60)
        };
    }

    private static List<Recommendation> GenerateRecommendations( 
        MetricsSummary summary)
    {
        var recommendations = new List<Recommendation>();

        // Анализ временных файлов
        if (summary.TempFilesPerMinute > 2) // > 2 файлов в минуту
        {
            recommendations.Add(new Recommendation
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
            recommendations.Add(new Recommendation
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
            recommendations.Add(new Recommendation
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

// TODO комменты потом
public class TempFilesStats
{
    public DateTime MeasurementTime { get; set; }
    public long TempFiles { get; set; }
    public long TempBytes { get; set; }
}