using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SqlAnalyzer.Api.Dal.Entities.Base;
using SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

namespace SqlAnalyzer.Api.Dal;

using Entities.DbConnection;

public class DataContext: DbContext
{
    public DbSet<DbConnection> DbConnections { get; set; }
    public DbSet<QueryAnalysis> QueryAnalysis { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
        UseAutoTimeStamps();
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