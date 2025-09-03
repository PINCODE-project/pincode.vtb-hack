using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;
using SqlAnalyzer.Api.Services.DbConnection.Interfaces;

namespace SqlAnalyzer.Api.Services.DbConnection;

using Dal.Entities.DbConnection;

public class DbConnectionService : IDbConnectionService
{
    private readonly DataContext _db;

    public DbConnectionService(DataContext db)
    {
        _db = db;
    }

    public async Task<SimpleDto<Guid>> SaveAsync(DbConnectionCreateDto request)
    {
        var entity = new DbConnection
        {
            Host = request.Host,
            Port = request.Port,
            Database = request.Database,
            Username = request.Username,
            Password = request.Password
        };

        _db.DbConnections.Add(entity);
        await _db.SaveChangesAsync();

        return new SimpleDto<Guid>(entity.Id);
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
        return $"Host={connection.Host};Port={connection.Port};Database={connection.Database};Username={connection.Username};Password={connection.Password};Pooling=false;Timeout=3";
    }
}