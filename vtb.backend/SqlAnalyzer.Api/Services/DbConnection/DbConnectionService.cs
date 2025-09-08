using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Base;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection.Interfaces;

namespace SqlAnalyzer.Api.Services.DbConnection;

using Dal.Entities.DbConnection;

public class DbConnectionService : IDbConnectionService
{
    private readonly DataContext _db;
    private readonly IMonitoringService _monitoringService;
    private readonly IAutovacuumMonitoringService _autovacuumMonitoringService;

    public DbConnectionService(DataContext db,
        IMonitoringService monitoringService,
        IAutovacuumMonitoringService autovacuumMonitoringService)
    {
        _db = db;
        _monitoringService = monitoringService;
        _autovacuumMonitoringService = autovacuumMonitoringService;
    }

    public async Task<IReadOnlyCollection<DbConnectionDto>> Find(DbConnectionFindDto dto)
    {
        var query = _db.DbConnections.AsNoTracking();

        if (string.IsNullOrEmpty(dto.Search) == false)
        {
            query = query.Where(db =>
                EF.Functions.ILike(
                    EF.Functions.Collate(db.Name, Db.CollationName),
                    Db.ContainsPattern(dto.Search)
                )
                || EF.Functions.ILike(
                    EF.Functions.Collate(db.Host, Db.CollationName),
                    Db.ContainsPattern(dto.Search)
                )
                || EF.Functions.ILike(
                    EF.Functions.Collate(db.Port.ToString(), Db.CollationName),
                    Db.ContainsPattern(dto.Search)
                )
                || EF.Functions.ILike(
                    EF.Functions.Collate(db.Database, Db.CollationName),
                    Db.ContainsPattern(dto.Search)
                )
            );
        }

        query = query.UseLimiter(dto.Skip, dto.Take);
        var result = await query
            .Select(d => new DbConnectionDto(d.Id, d.Name, d.Host, d.Port, d.Database, d.Username))
            .ToListAsync();

        return result;
    }

    public async Task<SimpleDto<Guid>> SaveAsync(DbConnectionCreateDto request)
    {
        var entity = new DbConnection
        {
            Name = request.Name,
            Host = request.Host,
            Port = request.Port,
            Database = request.Database,
            Username = request.Username,
            Password = request.Password
        };

        _db.DbConnections.Add(entity);
        await _db.SaveChangesAsync();

        for (var i = 0; i < 2; i++)
        { 
            // Кастомно сохраняем данные чтобы не получить ошибку сразу при сохранении
            await _monitoringService.SaveCacheHitMetricsAsync(entity);
            await _monitoringService.SaveEfficiencyIndexListAsync(entity);
            await _monitoringService.SaveTableStatisticsListAsync(entity);
            await _monitoringService.SaveTempFilesMetricsAsync(entity);
            await _autovacuumMonitoringService.SaveAutovacuumMetricsAsync(entity);
        }

        return new SimpleDto<Guid>(entity.Id);
    }

    public async Task Update(DbConnectionUpdateDto dto)
    {
        var db = await _db.DbConnections.FirstAsync(db => db.Id == dto.Id);

        if (string.IsNullOrEmpty(dto.Name) == false)
        {
            db.Name = dto.Name;
        }

        if (string.IsNullOrEmpty(dto.Database) == false)
        {
            db.Database = dto.Database;
        }

        if (string.IsNullOrEmpty(dto.Host) == false)
        {
            db.Host = dto.Host;
        }

        if (dto.Port is not null)
        {
            db.Port = dto.Port.Value;
        }

        if (string.IsNullOrEmpty(dto.Username) == false)
        {
            db.Username = dto.Username;
        }

        if (string.IsNullOrEmpty(dto.Password) == false)
        {
            db.Password = dto.Password;
        }
        
        _db.DbConnections.Update(db);
        await _db.SaveChangesAsync();
    }

    public Task Delete(Guid Id)
    {
        throw new NotImplementedException();
    }

    public async Task<DbConnectionCheckDto> CheckAsync(Guid dbConnectionId)
    {
        var dbConnection = _db.DbConnections.FirstOrDefault(x => x.Id == dbConnectionId);
        if (dbConnection == null)
        {
            return new DbConnectionCheckDto(false, "DbConnection not found");
        }

        var connectionString =
            $"Host={dbConnection.Host};Port={dbConnection.Port};Database={dbConnection.Database};Username={dbConnection.Username};Password={dbConnection.Password};Pooling=false;Timeout=3";

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            return new DbConnectionCheckDto(true);
        }
        catch (Exception ex)
        {
            return new DbConnectionCheckDto(false, ex.Message);
        }
    }

    public static string GetConnectionString(DbConnection connection)
    {
        return
            $"Host={connection.Host};Port={connection.Port};Database={connection.Database};Username={connection.Username};Password={connection.Password};Pooling=false;Timeout=3";
    }
}