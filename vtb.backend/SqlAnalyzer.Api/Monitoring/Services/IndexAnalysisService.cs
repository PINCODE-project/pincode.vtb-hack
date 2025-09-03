using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public class IndexAnalysisService : IIndexAnalysisService
{
    private readonly DataContext _context;
    
    public IndexAnalysisService(DataContext context)
    {
        _context = context;
    }
    
    public async Task<IndexAnalysisReport> AnalyzeIndexesAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.IndexMetrics.AsQueryable();
        
        if (fromDate.HasValue)
            query = query.Where(x => x.CreateAt >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(x => x.CreateAt <= toDate.Value);
        
        var stats = await query.ToListAsync();
        
        return new IndexAnalysisReport
        {
            UnusedIndexes = FindUnusedIndexes(stats),
            InefficientIndexes = FindInefficientIndexes(stats),
            BloatedIndexes = FindBloatedIndexes(stats),
            MissingIndexes = FindMissingIndexes(stats),
            TopLargeIndexes = FindTopLargeIndexes(stats),
            TablesWithDeadTuples = FindTablesWithDeadTuples(stats),
            StatisticsSummary = GenerateSummary(stats)
        };
    }
    
    private List<IndexProblem> FindUnusedIndexes(List<IndexMetric> stats)
    {
        return stats
            .Where(x => x.IndexStatus == "UNUSED_INDEX")
            .GroupBy(x => new { x.SchemaName, x.TableName, x.IndexName })
            .Select(g => new IndexProblem
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                ProblemType = "UNUSED_INDEX",
                Severity = "HIGH",
                Description = $"Index has never been used. Size: {g.First().IndexSize}",
                Recommendation = "Consider dropping this index to save space and improve write performance",
                Occurrences = g.Count(),
                AvgMetricValue = g.Average(x => x.IndexScans)
            })
            .ToList();
    }
    
    private List<IndexProblem> FindInefficientIndexes(List<IndexMetric> stats)
    {
        return stats
            .Where(x => x.IndexStatus == "INEFFICIENT_INDEX" && x.IndexEfficiency < 10)
            .GroupBy(x => new { x.SchemaName, x.TableName, x.IndexName })
            .Select(g => new IndexProblem
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                ProblemType = "INEFFICIENT_INDEX",
                Severity = "MEDIUM",
                Description = $"Low efficiency: {g.Average(x => x.IndexEfficiency):F2}%",
                Recommendation = "Review index usage, consider partial indexes or redesign",
                Occurrences = g.Count(),
                AvgMetricValue = g.Average(x => x.IndexEfficiency)
            })
            .ToList();
    }
    
    private List<IndexProblem> FindBloatedIndexes(List<IndexMetric> stats)
    {
        return stats
            .Where(x => x.BloatFactor > 50 && x.IndexScans > 0)
            .GroupBy(x => new { x.SchemaName, x.TableName, x.IndexName })
            .Select(g => new IndexProblem
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = g.Key.IndexName,
                ProblemType = "BLOATED_INDEX",
                Severity = "MEDIUM",
                Description = $"High bloat factor: {g.Average(x => x.BloatFactor):F2}%",
                Recommendation = "Consider REINDEX or VACUUM FULL during maintenance window",
                Occurrences = g.Count(),
                AvgMetricValue = g.Average(x => x.BloatFactor)
            })
            .ToList();
    }
    
    private List<IndexProblem> FindMissingIndexes(List<IndexMetric> stats)
    {
        return stats
            .Where(x => x.SeqScanRatio > 0.5 && x.SequentialScans > 1000)
            .GroupBy(x => new { x.SchemaName, x.TableName })
            .Select(g => new IndexProblem
            {
                SchemaName = g.Key.SchemaName,
                TableName = g.Key.TableName,
                IndexName = "N/A",
                ProblemType = "MISSING_INDEX",
                Severity = "HIGH",
                Description = $"High sequential scans: {g.Average(x => x.SequentialScans):F0}, Ratio: {g.Average(x => x.SeqScanRatio):P2}",
                Recommendation = "Analyze query patterns and add indexes on frequently filtered columns",
                Occurrences = g.Count(),
                AvgMetricValue = g.Average(x => x.SeqScanRatio)
            })
            .ToList();
    }
    
    private List<IndexProblem> FindTopLargeIndexes(List<IndexMetric> stats)
    {
        return stats
            .GroupBy(x => new { x.SchemaName, x.TableName, x.IndexName })
            .Select(g => new
            {
                Key = g.Key,
                AvgSize = g.Average(x => ParseSize(x.IndexSize)),
                Stats = g.First()
            })
            .Where(x => x.AvgSize > 100 * 1024 * 1024) // >100MB
            .OrderByDescending(x => x.AvgSize)
            .Take(10)
            .Select(x => new IndexProblem
            {
                SchemaName = x.Key.SchemaName,
                TableName = x.Key.TableName,
                IndexName = x.Key.IndexName,
                ProblemType = "LARGE_INDEX",
                Severity = "LOW",
                Description = $"Large index size: {FormatSize(x.AvgSize)}",
                Recommendation = "Monitor growth, consider partitioning or index optimization",
                Occurrences = 1,
                AvgMetricValue = x.AvgSize
            })
            .ToList();
    }
    
    private List<IndexProblem> FindTablesWithDeadTuples(List<IndexMetric> stats)
    {
        return stats
            .GroupBy(x => new { x.SchemaName, x.TableName })
            .Select(g => new
            {
                Key = g.Key,
                AvgDeadRatio = g.Average(x => x.DeadTupleRatio),
                AvgDeadTuples = g.Average(x => x.DeadTuples),
                Stats = g.First()
            })
            .Where(x => x.AvgDeadRatio > 0.2) // >20% dead tuples
            .OrderByDescending(x => x.AvgDeadRatio)
            .Take(10)
            .Select(x => new IndexProblem
            {
                SchemaName = x.Key.SchemaName,
                TableName = x.Key.TableName,
                IndexName = "N/A",
                ProblemType = "HIGH_DEAD_TUPLES",
                Severity = "MEDIUM",
                Description = $"High dead tuple ratio: {x.AvgDeadRatio:P2}, Count: {x.AvgDeadTuples:F0}",
                Recommendation = "Consider tuning autovacuum settings or manual VACUUM",
                Occurrences = 1,
                AvgMetricValue = x.AvgDeadRatio
            })
            .ToList();
    }
    
    private StatisticsSummary GenerateSummary(List<IndexMetric> stats)
    {
        var grouped = stats.GroupBy(x => new { x.SchemaName, x.TableName, x.IndexName });
        
        return new StatisticsSummary
        {
            TotalIndexes = grouped.Count(),
            TotalUnusedIndexes = grouped.Count(g => g.Any(x => x.IndexStatus == "UNUSED_INDEX")),
            TotalInefficientIndexes = grouped.Count(g => g.Any(x => x.IndexStatus == "INEFFICIENT_INDEX")),
            AverageEfficiency = stats.Where(x => x.IndexEfficiency > 0).Average(x => x.IndexEfficiency),
            TotalSequentialScans = stats.Sum(x => x.SequentialScans),
            MaxDeadTupleRatio = stats.Max(x => x.DeadTupleRatio),
            DataCollectionPeriod = new DateRange(stats.Min(x => x.CreateAt), stats.Max(x => x.CreateAt))
        };
    }
    
    private double ParseSize(string sizeString)
    {
        if (string.IsNullOrEmpty(sizeString)) return 0;
        
        var parts = sizeString.Split(' ');
        if (parts.Length != 2) return 0;
        
        if (!double.TryParse(parts[0], out var size)) return 0;
        
        return parts[1].ToUpper() switch
        {
            "KB" => size * 1024,
            "MB" => size * 1024 * 1024,
            "GB" => size * 1024 * 1024 * 1024,
            "TB" => size * 1024 * 1024 * 1024 * 1024,
            _ => size
        };
    }
    
    private string FormatSize(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes /= 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
}

// Классы для результатов анализа
public class IndexAnalysisReport
{
    public List<IndexProblem> UnusedIndexes { get; set; } = new List<IndexProblem>();
    public List<IndexProblem> InefficientIndexes { get; set; } = new List<IndexProblem>();
    public List<IndexProblem> BloatedIndexes { get; set; } = new List<IndexProblem>();
    public List<IndexProblem> MissingIndexes { get; set; } = new List<IndexProblem>();
    public List<IndexProblem> TopLargeIndexes { get; set; } = new List<IndexProblem>();
    public List<IndexProblem> TablesWithDeadTuples { get; set; } = new List<IndexProblem>();
    public StatisticsSummary StatisticsSummary { get; set; } = new StatisticsSummary();
}

public class IndexProblem
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public string ProblemType { get; set; }
    public string Severity { get; set; }
    public string Description { get; set; }
    public string Recommendation { get; set; }
    public int Occurrences { get; set; }
    public double AvgMetricValue { get; set; }
}

public class StatisticsSummary
{
    public int TotalIndexes { get; set; }
    public int TotalUnusedIndexes { get; set; }
    public int TotalInefficientIndexes { get; set; }
    public double AverageEfficiency { get; set; }
    public long TotalSequentialScans { get; set; }
    public double MaxDeadTupleRatio { get; set; }
    public DateRange DataCollectionPeriod { get; set; }
}

public record DateRange(DateTime Start, DateTime End);