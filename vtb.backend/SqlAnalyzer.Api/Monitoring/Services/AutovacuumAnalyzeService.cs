using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

public class AutovacuumAnalysisService : IAutovacuumAnalysisService
{
    private readonly ILogger<AutovacuumAnalysisService> _logger;
    private readonly DataContext _context;

    public AutovacuumAnalysisService(
        ILogger<AutovacuumAnalysisService> logger, 
        DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<AutovacuumAnalysisResponse> AnalyzeAutovacuumLastHourAsync()
    {
        var response = new AutovacuumAnalysisResponse
        {
            AnalysisPeriodEnd = DateTime.UtcNow,
            AnalysisPeriodStart = DateTime.UtcNow.AddDays(-1)
        };

        try
        {
            var recentStats = await GetRecentStats();
            response.ProblematicTables = IdentifyProblematicTables(recentStats);
            response.MetricsSummary = CalculateMetricsSummary(recentStats, response.ProblematicTables);
            response.Recommendations = GenerateRecommendations(response.ProblematicTables, response.MetricsSummary);
            response.OverallStatus = DetermineOverallStatus(response.MetricsSummary);
                
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе autovacuum");
            response.OverallStatus = "error";
            return response;
        }
    }

    private async Task<List<AutovacuumStat>> GetRecentStats()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        return await _context.AutovacuumStats
            .Where(s => s.CreateAt >= oneHourAgo)
            .OrderByDescending(s => s.CreateAt)
            .ThenBy(s => s.SchemaName)
            .ThenBy(s => s.TableName)
            .ToListAsync();
    }

    private List<ProblematicTable> IdentifyProblematicTables(List<AutovacuumStat> stats)
    {
        var problematic = new List<ProblematicTable>();
        var latestStats = stats.GroupBy(s => new { s.SchemaName, s.TableName })
            .Select(g => g.OrderByDescending(s => s.CreateAt).First())
            .ToList();

        foreach (var stat in latestStats)
        {
            // Критерии основаны на процентных соотношениях
            var isProblematic = stat.DeadTupleRatio > 10m || 
                                stat.ChangeRatePercent > 50m; // Рост > 50% в час

            if (isProblematic)
            {
                var priority = DeterminePriority(stat.DeadTupleRatio, stat.ChangeRatePercent);
                var growthTrend = DetermineGrowthTrend(stat.ChangeRatePercent);

                problematic.Add(new ProblematicTable
                {
                    SchemaName = stat.SchemaName,
                    TableName = stat.TableName,
                    LiveTuples = stat.LiveTuples,
                    DeadTuples = stat.DeadTuples,
                    DeadTupleRatio = stat.DeadTupleRatio,
                    ChangeRatePercent = stat.ChangeRatePercent,
                    TableSize = stat.TableSize,
                    LastAutoVacuum = stat.LastAutoVacuum ?? DateTime.MinValue,
                    Priority = priority,
                    GrowthTrend = growthTrend
                });
            }
        }

        return problematic.OrderByDescending(t => t.DeadTupleRatio).ToList();
    }

    private string DeterminePriority(decimal deadTupleRatio, decimal changeRatePercent)
    {
        if (deadTupleRatio > 40m || changeRatePercent > 200m) return "critical";
        if (deadTupleRatio > 25m || changeRatePercent > 100m) return "high";
        if (deadTupleRatio > 15m || changeRatePercent > 50m) return "medium";
        return "low";
    }

    private string DetermineGrowthTrend(decimal changeRatePercent)
    {
        return changeRatePercent switch
        {
            > 100m => "rapid",
            > 50m => "moderate",
            > 20m => "slow",
            _ => "stable"
        };
    }

    private List<AutovacuumRecommendation> GenerateRecommendations(
        List<ProblematicTable> problematicTables, 
        AutovacuumMetricsSummary metrics)
    {
        var recommendations = new List<AutovacuumRecommendation>();

        // Системные рекомендации на основе процентных соотношений
        if (metrics.SystemWideDeadTupleRatio > 15m)
        {
            recommendations.Add(new AutovacuumRecommendation
            {
                Type = "global_tuning",
                Severity = "high",
                Message = $"Высокий системный уровень мертвых tuples: {metrics.SystemWideDeadTupleRatio}%",
                Parameter = "autovacuum_vacuum_scale_factor",
                CurrentValue = "0.2",
                RecommendedValue = "0.1",
                SqlCommand = "ALTER SYSTEM SET autovacuum_vacuum_scale_factor = 0.1;"
            });
        }

        if (metrics.TablesAbove20Percent > metrics.TotalTables * 0.3m) // >30% таблиц с >20% dead tuples
        {
            recommendations.Add(new AutovacuumRecommendation
            {
                Type = "workers_capacity",
                Severity = "medium",
                Message = $"Много проблемных таблиц ({metrics.TablesAbove20Percent} из {metrics.TotalTables})",
                Parameter = "autovacuum_max_workers",
                CurrentValue = "3",
                RecommendedValue = "5",
                SqlCommand = "ALTER SYSTEM SET autovacuum_max_workers = 5;"
            });
        }

        // Рекомендации для конкретных таблиц
        foreach (var table in problematicTables.Where(t => t.Priority == "critical"))
        {
            var scaleFactor = table.DeadTupleRatio > 50m ? 0.02m : 0.05m;
                
            recommendations.Add(new AutovacuumRecommendation
            {
                Type = "table_specific",
                Severity = "critical",
                Message = $"Критическая таблица: {table.TableName} ({table.DeadTupleRatio}% dead tuples, рост {table.ChangeRatePercent}%/час)",
                TableName = table.TableName,
                Parameter = "autovacuum_vacuum_scale_factor",
                CurrentValue = "0.2",
                RecommendedValue = scaleFactor.ToString("F2"),
                SqlCommand = $"ALTER TABLE {table.SchemaName}.{table.TableName} SET (autovacuum_vacuum_scale_factor = {scaleFactor});"
            });
        }

        return recommendations;
    }

    private AutovacuumMetricsSummary CalculateMetricsSummary(List<AutovacuumStat> stats, List<ProblematicTable> problematicTables)
    {
        var latestStats = stats.GroupBy(s => new { s.SchemaName, s.TableName })
            .Select(g => g.OrderByDescending(s => s.CreateAt).First())
            .ToList();

        var summary = new AutovacuumMetricsSummary
        {
            TotalTables = latestStats.Count,
            ProblematicTables = problematicTables.Count,
            CriticalTables = problematicTables.Count(t => t.Priority == "critical"),
            TotalLiveTuples = latestStats.Sum(s => s.LiveTuples),
            TotalDeadTuples = latestStats.Sum(s => s.DeadTuples),
            AvgDeadTupleRatio = latestStats.Average(s => s.DeadTupleRatio),
            MaxDeadTupleRatio = latestStats.Max(s => s.DeadTupleRatio),
            TablesAbove10Percent = latestStats.Count(s => s.DeadTupleRatio > 10m),
            TablesAbove20Percent = latestStats.Count(s => s.DeadTupleRatio > 20m),
            TablesAbove30Percent = latestStats.Count(s => s.DeadTupleRatio > 30m),
            AvgChangeRatePercent = latestStats.Average(s => s.ChangeRatePercent)
        };

        if (problematicTables.Any())
        {
            var worstTable = problematicTables.OrderByDescending(t => t.DeadTupleRatio).First();
            summary.WorstTable = worstTable.TableName;
            summary.WorstTableRatio = worstTable.DeadTupleRatio;
        }

        return summary;
    }

    private string DetermineOverallStatus(AutovacuumMetricsSummary metrics)
    {
        if (metrics.SystemWideDeadTupleRatio > 25m) return "critical";
        if (metrics.SystemWideDeadTupleRatio > 15m) return "high";
        if (metrics.SystemWideDeadTupleRatio > 10m) return "warning";
        if (metrics.TablesAbove20Percent > metrics.TotalTables * 0.2m) return "attention";
        return "healthy";
    }
}