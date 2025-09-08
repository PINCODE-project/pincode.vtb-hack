using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.DbConnection;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

public class AutovacuumMonitoringService : IAutovacuumMonitoringService
{
    private readonly ILogger<AutovacuumMonitoringService> _logger;
    private readonly DataContext _context;

    public AutovacuumMonitoringService(
        ILogger<AutovacuumMonitoringService> logger, 
        DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<bool> SaveAutovacuumMetricsAsync(DbConnection connectionString)
    {
        try
        {
            await using var targetConn = new NpgsqlConnection(connectionString.GetConnectionString());
            await targetConn.OpenAsync();

            var currentStats = await GetCurrentTableStats(targetConn);
            var previousStats = await GetPreviousStatsForComparison();
                
            var statsWithTrends = CalculateTrends(currentStats, previousStats);
            await SaveStatsToDatabase(statsWithTrends, connectionString.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении метрик autovacuum");
            return false;
        }
    }

    private async Task<List<TableVacuumStats>> GetCurrentTableStats(NpgsqlConnection connection)
    {
        var stats = new List<TableVacuumStats>();
            
        var query = @"
                SELECT 
                    schemaname,
                    relname,
                    n_live_tup,
                    n_dead_tup,
                    last_autovacuum,
                    pg_total_relation_size(relid) as table_size,
                    n_tup_ins,
                    n_tup_upd,
                    n_tup_del
                FROM pg_stat_all_tables 
                WHERE schemaname NOT LIKE 'pg_%' AND schemaname != 'information_schema'
                ORDER BY n_dead_tup DESC";

        await using var cmd = new NpgsqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
            
        while (await reader.ReadAsync())
        {
            stats.Add(new TableVacuumStats
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                LiveTuples = reader.GetInt64(2),
                DeadTuples = reader.GetInt64(3),
                LastAutoVacuum = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                TableSize = reader.GetInt64(5),
                Inserts = reader.GetInt64(6),
                Updates = reader.GetInt64(7),
                Deletes = reader.GetInt64(8)
            });
        }

        return stats;
    }

    private async Task<List<AutovacuumStat>> GetPreviousStatsForComparison()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        return await _context.AutovacuumStats
            .Where(s => s.CreateAt >= oneHourAgo)
            .ToListAsync();
    }

    private List<AutovacuumStat> CalculateTrends(List<TableVacuumStats> currentStats, List<AutovacuumStat> previousStats)
    {
        var result = new List<AutovacuumStat>();
        var now = DateTime.UtcNow;

        foreach (var current in currentStats)
        {
            var previous = previousStats.FirstOrDefault(p => 
                p.SchemaName == current.SchemaName && p.TableName == current.TableName);

            decimal changeRatePercent = 0;
            if (previous != null && previous.DeadTuples > 0)
            {
                var deadTuplesChange = current.DeadTuples - previous.DeadTuples;
                changeRatePercent = (decimal)deadTuplesChange / previous.DeadTuples * 100;
            }

            var deadTupleRatio = current.LiveTuples > 0 ? 
                (decimal)current.DeadTuples / current.LiveTuples * 100 : 0;

            result.Add(new AutovacuumStat
            {
                Id = Guid.NewGuid(),
                CreateAt = now,
                SchemaName = current.SchemaName,
                TableName = current.TableName,
                LiveTuples = current.LiveTuples,
                DeadTuples = current.DeadTuples,
                DeadTupleRatio = Math.Round(deadTupleRatio, 2),
                TableSize = current.TableSize,
                LastAutoVacuum = current.LastAutoVacuum,
                ChangeRatePercent = Math.Round(changeRatePercent, 2)
            });
        }

        return result;
    }

    private async Task SaveStatsToDatabase(List<AutovacuumStat> stats, Guid dbConnectionId)
    {
        foreach (var stat in stats)
        {
            stat.DbConnectionId = dbConnectionId;
        }
        await _context.AutovacuumStats.AddRangeAsync(stats);
        await _context.SaveChangesAsync();
    }
}

public class TableVacuumStats
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long LiveTuples { get; set; }
    public long DeadTuples { get; set; }
    public DateTime LastAutoVacuum { get; set; }
    public long TableSize { get; set; }
    public long Inserts { get; set; }
    public long Updates { get; set; }
    public long Deletes { get; set; }
}