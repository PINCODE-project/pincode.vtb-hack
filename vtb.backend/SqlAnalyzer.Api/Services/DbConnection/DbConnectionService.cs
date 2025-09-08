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

/// <inheritdoc />
public class DbConnectionService : IDbConnectionService
{
    private readonly DataContext _db;
    private readonly IMonitoringService _monitoringService;
    private readonly IAutovacuumMonitoringService _autovacuumMonitoringService;

    /// 
    public DbConnectionService(DataContext db,
        IMonitoringService monitoringService,
        IAutovacuumMonitoringService autovacuumMonitoringService)
    {
        _db = db;
        _monitoringService = monitoringService;
        _autovacuumMonitoringService = autovacuumMonitoringService;
    }

    /// <inheritdoc />
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
            .Select(d => new DbConnectionDto
            {
                Id = d.Id, Name = d.Name, Host = d.Host, Port = d.Port, Database = d.Database, Username = d.Username
            })
            .ToListAsync();

        return result;
    }

    /// <inheritdoc />
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
            //await _monitoringService.SaveTableStatisticsListAsync(entity);
            await _monitoringService.SaveTempFilesMetricsAsync(entity);
            await _autovacuumMonitoringService.SaveAutovacuumMetricsAsync(entity);
        }

        return new SimpleDto<Guid>(entity.Id);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task Delete(Guid id)
    {
        await _db.DbConnections.Where(db => db.Id == id).ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task<DbConnectionCheckDto> CheckAsync(DbConnectionCreateDto dto)
    {
        var connectionString = GetConnectionString(dto.Host, dto.Port, dto.Database, dto.Username, dto.Password);
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            return new DbConnectionCheckDto { IsValid = true };
        }
        catch (Exception ex)
        {
            return new DbConnectionCheckDto { IsValid = false, ErrorMessage = ex.Message };
        }
    }

    private static string GetConnectionString(string host, int port, string database, string username, string password)
    {
        return
            $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling=false;Timeout=3";
    }
}