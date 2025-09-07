using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

internal class IndexAnalysisService : IIndexAnalysisService
{
    private readonly DataContext _context;
    
    public IndexAnalysisService(DataContext context)
    {
        _context = context;
    }

    public async Task<IndexAnalysisResult> GetFullAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        var metrics = await _context.IndexMetrics.Where(x => x.CreateAt >= startDate && x.CreateAt <= endDate).ToListAsync();
        if (metrics.Count == 0)
        {
            return new IndexAnalysisResult();
        }
        return new IndexAnalysisResult
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            UnusedIndexes = FindUnusedIndexes(metrics),
            InefficientIndexes = FindInefficientIndexes(metrics),
            GrowingIndexes = FindGrowingIndexes(metrics),
            MostUsedIndexes = FindMostUsedIndexes(metrics),
            IndexUsageStatistics = GetUsageStatistics(metrics)
        };
    }

    public List<IndexRecommendation> FindUnusedIndexes(List<IndexMetric> metrics, long minScansThreshold = 10)
    {
        var recommendations = metrics
            .GroupBy(m => new { m.SchemaName, m.TableName, m.IndexName })
            .Where(g => g.Average(m => m.IndexScans) <= minScansThreshold)
            .Select(g => new IndexRecommendation
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                MetricType = "UnusedIndex",
                Severity = GetUnusedIndexSeverity(g.Average(m => m.IndexScans)),
                AverageValue = (double)g.Average(m => m.IndexScans),
                MaxSize = g.Max(m => m.IndexSize),
                Recommendation = GenerateUnusedIndexRecommendation(g.Key.IndexName, g.Average(m => m.IndexScans), g.Max(m => m.IndexSize)),
                DataPoints = g.OrderBy(m => m.CreateAt).ToList()
            })
            .OrderByDescending(r => r.MaxSize) // Сначала большие индексы
            .ToList();

        return recommendations;
    }

    public List<IndexRecommendation> FindInefficientIndexes(List<IndexMetric> metrics, double efficiencyThreshold = 30.0, long minScans = 50)
    {
        var recommendations = metrics
            .Where(m => m.IndexScans >= minScans)
            .GroupBy(m => new { m.SchemaName, m.TableName, m.IndexName })
            .Where(g => g.Average(m => m.Efficiency) <= efficiencyThreshold)
            .Select(g => new IndexRecommendation
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                MetricType = "InefficientIndex",
                Severity = GetEfficiencySeverity(g.Average(m => m.Efficiency)),
                AverageValue = g.Average(m => m.Efficiency),
                MaxSize = g.Max(m => m.IndexSize),
                Recommendation = GenerateInefficientIndexRecommendation(g.Key.IndexName, g.Average(m => m.Efficiency), 
                                                                       g.Average(m => m.TuplesRead), g.Average(m => m.TuplesFetched)),
                DataPoints = g.OrderBy(m => m.CreateAt).ToList()
            })
            .OrderBy(r => r.AverageValue) // Сначала самые неэффективные
            .ToList();

        return recommendations;
    }

    public List<IndexRecommendation> FindGrowingIndexes(List<IndexMetric> metrics, double growthPercentageThreshold = 15.0, long minSize = 1024 * 1024) // 1MB
    {
        var recommendations = metrics
            .GroupBy(m => new { m.SchemaName, m.TableName, m.IndexName })
            .Where(g => g.Count() > 1 && g.Max(m => m.IndexSize) >= minSize)
            .Select(g =>
            {
                var first = g.OrderBy(m => m.CreateAt).First();
                var last = g.OrderByDescending(m => m.CreateAt).First();
                var growthPercentage = CalculateGrowthPercentage(first.IndexSize, last.IndexSize);

                return new IndexRecommendation
                {
                    SchemaName = g.Key.SchemaName,
                    TableName = g.Key.TableName,
                    IndexName = g.Key.IndexName,
                    MetricType = "GrowingIndex",
                    Severity = GetGrowthSeverity(growthPercentage),
                    AverageValue = (double)growthPercentage,
                    MaxSize = last.IndexSize,
                    Recommendation = GenerateGrowingIndexRecommendation(g.Key.IndexName, first.IndexSize, last.IndexSize, growthPercentage),
                    DataPoints = g.OrderBy(m => m.CreateAt).ToList()
                };
            })
            .Where(r => r.AverageValue >= (double)growthPercentageThreshold)
            .OrderByDescending(r => r.AverageValue)
            .ToList();

        return recommendations;
    }

    public List<IndexUsage> FindMostUsedIndexes(List<IndexMetric> metrics, int topN = 10)
    {
        var mostUsed = metrics
            .GroupBy(m => new { m.SchemaName, m.TableName, m.IndexName })
            .Select(g => new IndexUsage
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                AverageScans = (long)g.Average(m => m.IndexScans),
                TotalScans = g.Sum(m => m.IndexScans),
                AverageEfficiency = (double)g.Average(m => m.Efficiency),
                Recommendation = GenerateUsageRecommendation(g.Key.IndexName, g.Average(m => m.IndexScans), g.Average(m => m.Efficiency))
            })
            .OrderByDescending(u => u.TotalScans)
            .Take(topN)
            .ToList();

        return mostUsed;
    }

    public IndexUsageStatistics GetUsageStatistics(List<IndexMetric> metrics)
    {
        var statistics = new IndexUsageStatistics
        {
            TotalIndexes = metrics.Select(m => new { m.SchemaName, m.TableName, m.IndexName }).Distinct().Count(),
            TotalScans = metrics.Sum(m => m.IndexScans),
            AverageEfficiency = metrics.Average(m => m.Efficiency),
            IndexesByEfficiency = metrics
                .GroupBy(m => new { m.SchemaName, m.TableName, m.IndexName })
                .Select(g => new { Efficiency = g.Average(m => m.Efficiency) })
                .GroupBy(x => x.Efficiency switch
                {
                    < 10 => "Very Low (<10%)",
                    < 30 => "Low (10-30%)",
                    < 60 => "Medium (30-60%)", 
                    < 90 => "High (60-90%)",
                    _ => "Excellent (90-100%)"
                })
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return statistics;
    }

    private string GenerateUnusedIndexRecommendation(string indexName, double averageScans, long maxSize)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Индекс '{indexName}' практически не используется (в среднем {averageScans:F1} сканирований в час).");
        
        if (maxSize > 100 * 1024 * 1024) // > 100MB
        {
            sb.AppendLine($"СЕРЬЕЗНАЯ ПРОБЛЕМА: Индекс занимает {FormatBytes(maxSize)} и не используется.");
            sb.AppendLine("РЕКОМЕНДАЦИИ:");
            sb.AppendLine("УДАЛИТЬ индекс: DROP INDEX CONCURRENTLY IF EXISTS {index_name};");
            sb.AppendLine("Проверить, не используется ли индекс для ограничений UNIQUE или FOREIGN KEY");
            sb.AppendLine("Проанализировать workload - возможно, изменилась логика приложения");
        }
        else if (maxSize > 10 * 1024 * 1024) // > 10MB
        {
            sb.AppendLine($"ПРОБЛЕМА: Индекс занимает {FormatBytes(maxSize)} при минимальном использовании.");
            sb.AppendLine("РЕКОМЕНДАЦИИ:");
            sb.AppendLine("Рассмотреть возможность удаления индекса");
            sb.AppendLine("Убедиться, что индекс не нужен для редких отчетных запросов");
        }
        else
        {
            sb.AppendLine("ЗАМЕЧАНИЕ: Неиспользуемый индекс малого размера.");
            sb.AppendLine("РЕКОМЕНДАЦИИ:");
            sb.AppendLine("Можно оставить, если размер не критичен");
            sb.AppendLine("Удалить при необходимости освобождения места");
        }

        return sb.ToString();
    }

    private string GenerateInefficientIndexRecommendation(string indexName, double efficiency, double avgTuplesRead, double avgTuplesFetched)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Индекс '{indexName}' имеет низкую эффективность ({efficiency:F1}%).");
        sb.AppendLine($"Статистика: {avgTuplesRead:F0} строк прочитано → {avgTuplesFetched:F0} строк возвращено");

        sb.AppendLine("ВОЗМОЖНЫЕ ПРИЧИНЫ:");
        sb.AppendLine("Неселективный индекс (мало уникальных значений)");
        sb.AppendLine("Index scan вместо Index Only Scan");
        sb.AppendLine("Неоптимальный порядок колонок в индексе");
        sb.AppendLine("Несоответствие индекса условиям WHERE/JOIN");

        sb.AppendLine("РЕКОМЕНДАЦИИ:");
        
        if (efficiency < 10)
        {
            sb.AppendLine("КРИТИЧЕСКИ НИЗКАЯ ЭФФЕКТИВНОСТЬ");
            sb.AppendLine("Пересмотреть необходимость индекса");
            sb.AppendLine("Проверить селективность колонок");
            sb.AppendLine("Рассмотреть создание более селективного индекса");
        }
        else if (efficiency < 30)
        {
            sb.AppendLine("Добавить более селективные колонки в индекс");
            sb.AppendLine("Использовать partial index для фильтрации по частым значениям");
            sb.AppendLine("Проверить статистику и выполнить ANALYZE таблицы");
        }
        else
        {
            sb.AppendLine("Оптимизировать порядок колонок в индексе");
            sb.AppendLine("Рассмотреть использование covering index");
            sb.AppendLine("Проверить условия запросов, использующих этот индекс");
        }

        sb.AppendLine("Проанализировать план запросов: EXPLAIN (ANALYZE, BUFFERS) ...");

        return sb.ToString();
    }

    private string GenerateGrowingIndexRecommendation(string indexName, long startSize, long endSize, double growthPercentage)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Индекс '{indexName}' быстро растет: +{growthPercentage:F1}% за период");
        sb.AppendLine($"Размер: {FormatBytes(startSize)} → {FormatBytes(endSize)}");

        sb.AppendLine("ВОЗМОЖНЫЕ ПРИЧИНЫ:");
        sb.AppendLine("Частые INSERT/UPDATE операции по индексируемым полям");
        sb.AppendLine("Высокая фрагментация индекса");
        sb.AppendLine("Неоптимальный fillfactor");
        sb.AppendLine("MVCC накладные расходы (частое обновление строк)");

        sb.AppendLine("РЕКОМЕНДАЦИИ:");
        
        if (growthPercentage > 50)
        {
            sb.AppendLine("КРИТИЧЕСКИЙ РОСТ");
            sb.AppendLine("Выполнить REINDEX INDEX CONCURRENTLY {index_name}");
            sb.AppendLine("Оптимизировать fillfactor для частых обновлений");
            sb.AppendLine("Рассмотреть partitioning таблицы");
        }
        
        sb.AppendLine("Запланировать регулярное обслуживание: REINDEX");
        sb.AppendLine("Мониторить фрагментацию индекса");
        sb.AppendLine("Проверить паттерны запросов на предмет частых обновлений");

        if (endSize > 10 * 1024 * 1024 * 1024L) // > 10GB
        {
            sb.AppendLine("   💾 ОЧЕНЬ БОЛЬШОЙ ИНДЕКС:");
            sb.AppendLine("Рассмотреть использование BRIN индексов для временных данных");
            sb.AppendLine("Оценить необходимость такого большого индекса");
        }

        return sb.ToString();
    }

    private string GenerateUsageRecommendation(string indexName, double averageScans, double efficiency)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Индекс '{indexName}' интенсивно используется ({averageScans:F0} сканирований/час)");
        
        if (efficiency < 60)
        {
            sb.AppendLine($"ВНИМАНИЕ: Низкая эффективность ({efficiency:F1}%) при высокой нагрузке");
            sb.AppendLine("РЕКОМЕНДАЦИИ:");
            sb.AppendLine("Срочно оптимизировать структуру индекса");
            sb.AppendLine("Добавить недостающие колонки для covering index");
            sb.AppendLine("Проанализировать типичные запросы к этому индексу");
        }
        else
        {
            sb.AppendLine($"Хорошая эффективность: {efficiency:F1}%");
            sb.AppendLine("РЕКОМЕНДАЦИИ:");
            sb.AppendLine("Продолжать мониторить производительность");
            sb.AppendLine("Рассмотреть возможность рефакторинга запросов под этот индекс");
        }

        return sb.ToString();
    }

    private string GetUnusedIndexSeverity(double averageScans)
    {
        return averageScans switch
        {
            <= 1 => "Critical",
            <= 5 => "High",
            <= 10 => "Medium",
            _ => "Low"
        };
    }

    private string GetEfficiencySeverity(double efficiency)
    {
        return efficiency switch
        {
            < 10 => "Critical",
            < 30 => "High",
            < 50 => "Medium",
            < 70 => "Low",
            _ => "Info"
        };
    }

    private string GetGrowthSeverity(double growthPercentage)
    {
        return growthPercentage switch
        {
            > 100 => "Critical",
            > 50 => "High",
            > 25 => "Medium",
            > 15 => "Low",
            _ => "Info"
        };
    }

    private double CalculateGrowthPercentage(long startSize, long endSize)
    {
        if (startSize == 0) return endSize > 0 ? 100.0 : 0.0;
        return ((endSize - (double)startSize) / startSize) * 100.0;
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        double number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n2} {suffixes[counter]}";
    }
}

// Обновленные модели для результатов
public class IndexRecommendation
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public string MetricType { get; set; } // UnusedIndex, InefficientIndex, GrowingIndex
    public string Severity { get; set; } // Critical, High, Medium, Low, Info
    public double AverageValue { get; set; }
    public long MaxSize { get; set; }
    public string Recommendation { get; set; }
    public List<IndexMetric> DataPoints { get; set; } = new();
    
    public string FormattedSize => FormatBytes(MaxSize);
    
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        double number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n2} {suffixes[counter]}";
    }
}

public class IndexAnalysisResult
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<IndexRecommendation> UnusedIndexes { get; set; } = new();
    public List<IndexRecommendation> InefficientIndexes { get; set; } = new();
    public List<IndexRecommendation> GrowingIndexes { get; set; } = new();
    public List<IndexUsage> MostUsedIndexes { get; set; } = new();
    public IndexUsageStatistics IndexUsageStatistics { get; set; } = new();
}

public class IndexUsage
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public long AverageScans { get; set; }
    public long TotalScans { get; set; }
    public double AverageEfficiency { get; set; }
    public string Recommendation { get; set; }
}

public class IndexUsageStatistics
{
    /// <summary>
    /// Общее количество уникальных индексов в анализируемом наборе
    /// </summary>
    public int TotalIndexes { get; set; }
    
    /// <summary>
    /// Общее количество сканирований индексов за период
    /// </summary>
    public long TotalScans { get; set; }
    
    /// <summary>
    /// Средняя эффективность индексов в процентах
    /// </summary>
    public double AverageEfficiency { get; set; }
    
    /// <summary>
    /// Количество индексов по уровням эффективности
    /// </summary>
    public Dictionary<string, int> IndexesByEfficiency { get; set; } = new Dictionary<string, int>();
}