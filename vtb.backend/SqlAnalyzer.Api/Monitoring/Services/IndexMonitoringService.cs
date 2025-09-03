using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public class IndexMonitoringService : IIndexMonitoringService
{
    private readonly DataContext _context;
    
    public IndexMonitoringService(DataContext context)
    {
        _context = context;
    }
    
    public async Task CollectIndexStatisticsAsync(string connectionString)
    {
        var indexStats = await GetIndexStatisticsAsync(connectionString);
        var bloatStats = await GetBloatStatisticsAsync(connectionString);
        var seqScanStats = await GetSeqScanStatisticsAsync(connectionString);
        var tableStats = await GetTableStatisticsAsync(connectionString);
        
        var combinedStats = CombineStatistics(indexStats, bloatStats, seqScanStats, tableStats);
        
        await SaveStatisticsAsync(combinedStats);
    }
    
    private async Task<List<IndexStatRecord>> GetIndexStatisticsAsync(string connectionString)
    {
        var results = new List<IndexStatRecord>();
        
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(@"
            SELECT 
                schemaname, 
                relname, 
                indexrelname,
                idx_scan, 
                idx_tup_read, 
                idx_tup_fetch,
                pg_size_pretty(pg_relation_size(indexrelid)) as index_size,
                CASE 
                    WHEN idx_scan > 0 THEN 
                        (idx_tup_fetch::float / GREATEST(idx_tup_read, 1)) * 100 
                    ELSE 0 
                END as efficiency,
                CASE 
                    WHEN idx_scan = 0 AND pg_relation_size(indexrelid) > 1048576 THEN 
                        'UNUSED_INDEX'
                    WHEN idx_scan > 0 AND (idx_tup_fetch::float / GREATEST(idx_tup_read, 1)) < 0.1 THEN 
                        'INEFFICIENT_INDEX'
                    ELSE 'OK'
                END as status
            FROM pg_stat_all_indexes 
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')", connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new IndexStatRecord
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                IndexName = reader.GetString(2),
                IndexScans = reader.GetInt64(3),
                TuplesRead = reader.GetInt64(4),
                TuplesFetched = reader.GetInt64(5),
                IndexSize = reader.GetString(6),
                Efficiency = reader.GetDouble(7),
                Status = reader.GetString(8)
            });
        }
        
        return results;
    }
    
    private async Task<List<BloatStatRecord>> GetBloatStatisticsAsync(string connectionString)
    {
        var results = new List<BloatStatRecord>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
        SELECT
            i.schemaname,
            i.tablename,
            i.indexname,
            -- Используем CAST для округления через преобразование в numeric
            CAST(
                (CASE 
                    WHEN s.idx_scan > 0 THEN 
                        (pg_relation_size(i.indexname::regclass)::float / 
                         GREATEST(s.idx_scan * 100, 1)) 
                    ELSE 1 
                END) * 100 AS NUMERIC(10,2)
            ) AS bloat_factor
        FROM pg_indexes i
        JOIN pg_stat_all_indexes s ON i.indexname = s.indexrelname
        WHERE i.schemaname NOT IN ('pg_catalog', 'information_schema')", connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new BloatStatRecord
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                IndexName = reader.GetString(2),
                BloatFactor = reader.GetDecimal(3)
            });
        }
        
        return results;
    }
    
    private async Task<List<SeqScanRecord>> GetSeqScanStatisticsAsync(string connectionString)
    {
        var results = new List<SeqScanRecord>();
        
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(@"
            SELECT 
                schemaname,
                relname,
                seq_scan,
                seq_tup_read,
                n_live_tup,
                CASE 
                    WHEN n_live_tup > 0 THEN 
                        seq_scan::float / n_live_tup 
                    ELSE 0 
                END AS seq_scan_ratio
            FROM pg_stat_all_tables 
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')", connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SeqScanRecord
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                SequentialScans = reader.GetInt64(2),
                TuplesReadSeq = reader.GetInt64(3),
                LiveTuples = reader.GetInt64(4),
                SeqScanRatio = reader.GetDouble(5)
            });
        }
        
        return results;
    }
    
    private async Task<List<TableStatRecord>> GetTableStatisticsAsync(string connectionString)
    {
        var results = new List<TableStatRecord>();
        
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(@"
            SELECT 
                schemaname,
                relname,
                n_live_tup,
                n_dead_tup,
                CASE 
                    WHEN n_live_tup > 0 THEN 
                        n_dead_tup::float / n_live_tup 
                    ELSE 0 
                END AS dead_ratio
            FROM pg_stat_all_tables 
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')", connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new TableStatRecord
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                LiveTuples = reader.GetInt64(2),
                DeadTuples = reader.GetInt64(3),
                DeadRatio = reader.GetDouble(4)
            });
        }
        
        return results;
    }
    
    private List<IndexMetric> CombineStatistics(
        List<IndexStatRecord> indexStats,
        List<BloatStatRecord> bloatStats,
        List<SeqScanRecord> seqScanStats,
        List<TableStatRecord> tableStats)
    {
        var combined = new List<IndexMetric>();
        
        foreach (var stat in indexStats)
        {
            var bloat = bloatStats.FirstOrDefault(b => 
                b.SchemaName == stat.SchemaName && 
                b.TableName == stat.TableName &&
                b.IndexName == stat.IndexName);
            
            var seqScan = seqScanStats.FirstOrDefault(s => 
                s.SchemaName == stat.SchemaName && 
                s.TableName == stat.TableName);
            
            var tableStat = tableStats.FirstOrDefault(t => 
                t.SchemaName == stat.SchemaName && 
                t.TableName == stat.TableName);
            
            combined.Add(new IndexMetric
            {
                CreateAt = DateTime.UtcNow.ToUniversalTime(),
                SchemaName = stat.SchemaName,
                TableName = stat.TableName,
                IndexName = stat.IndexName,
                IndexScans = stat.IndexScans,
                TuplesRead = stat.TuplesRead,
                TuplesFetched = stat.TuplesFetched,
                IndexSize = stat.IndexSize,
                IndexEfficiency = stat.Efficiency,
                IndexStatus = stat.Status,
                BloatFactor = (double)(bloat?.BloatFactor ?? 0),
                SequentialScans = seqScan?.SequentialScans ?? 0,
                SeqScanRatio = seqScan?.SeqScanRatio ?? 0,
                LiveTuples = tableStat?.LiveTuples ?? 0,
                DeadTuples = tableStat?.DeadTuples ?? 0,
                DeadTupleRatio = tableStat?.DeadRatio ?? 0
            });
        }
        
        return combined;
    }
    
    private async Task SaveStatisticsAsync(List<IndexMetric> statistics)
    {
        await _context.IndexMetrics.AddRangeAsync(statistics);
        await _context.SaveChangesAsync();
    }
    
    // Вспомогательные классы для временного хранения данных
    private class IndexStatRecord
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public long IndexScans { get; set; }
        public long TuplesRead { get; set; }
        public long TuplesFetched { get; set; }
        public string IndexSize { get; set; }
        public double Efficiency { get; set; }
        public string Status { get; set; }
    }
    
    private class BloatStatRecord
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public decimal BloatFactor { get; set; } // Изменяем на decimal
    }
    
    private class SeqScanRecord
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public long SequentialScans { get; set; }
        public long TuplesReadSeq { get; set; }
        public long LiveTuples { get; set; }
        public double SeqScanRatio { get; set; }
    }
    
    private class TableStatRecord
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public long LiveTuples { get; set; }
        public long DeadTuples { get; set; }
        public double DeadRatio { get; set; }
    }
}