using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;
using SqlAnalyzer.Api.Services.LlmClient.Data;
using SqlAnalyzerLib.Recommendation.Models;

namespace SqlAnalyzer.Api.Dal;

using Entities.DbConnection;

public class DataContext: DbContext
{
    public DbSet<DbConnection> DbConnections { get; set; }
    public DbSet<QueryAnalysis> Queries { get; set; }
    public DbSet<QueryAnalysisResult> QueryAnalysisResults { get; set; }
    public DbSet<CacheHitStats> CacheHitStats { get; set; }
    public DbSet<AutovacuumStat> AutovacuumStats { get; set; }
    
    public DbSet<TempFilesStatsDal> TempFilesStats { get; set; }
    public DbSet<IndexMetric> IndexMetrics { get; set; }
    public DbSet<TableStatictics> TableStatictics { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
        UseAutoTimeStamps();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QueryAnalysisResult>()
            .Property(x => x.Recommendations)
            .HasColumnType("jsonb");
        
        modelBuilder.Entity<QueryAnalysisResult>(entity =>
            entity.Property(x => x.Recommendations)
                .HasConversion(
                    w => JsonSerializer.Serialize(w, JsonSerializerOptions.Default),
                    w => JsonSerializer.Deserialize<IReadOnlyCollection<Recommendation>>(w, JsonSerializerOptions.Default)!)
                .HasColumnType("jsonb"));
        
        modelBuilder.Entity<QueryAnalysisResult>(entity =>
            entity.Property(x => x.LlmRecommendations)
                .HasConversion(
                    w => JsonSerializer.Serialize(w, JsonSerializerOptions.Default),
                    w => JsonSerializer.Deserialize<LlmAnswer>(w, JsonSerializerOptions.Default)!)
                .HasColumnType("jsonb"));
        
        base.OnModelCreating(modelBuilder);
    }


    private void UseAutoTimeStamps()
    {
        ChangeTracker.StateChanged += UpdateTimestamps;
        ChangeTracker.Tracked += UpdateTimestamps;
    }
    
    private static void UpdateTimestamps(object? sender, EntityEntryEventArgs e)
    {
        var entry = e.Entry;
        var entity = entry.Entity;

        switch (entry.State)
        {
            case EntityState.Modified when entity is IEntityCreatedAt:
            {
                // var now = DateTime.UtcNow;
                // entry.Property(nameof(IEntityCreatedAt.UpdateAt)).CurrentValue = now;
                break;
            }
            case EntityState.Added when entity is IEntityCreatedAt:
            {
                var now = DateTime.UtcNow;
                entry.Property(nameof(IEntityCreatedAt.CreateAt)).CurrentValue = now;
                //entry.Property(nameof(IEntityCreatedAt.UpdateAt)).CurrentValue = now;

                break;
            }
        }
    }
}