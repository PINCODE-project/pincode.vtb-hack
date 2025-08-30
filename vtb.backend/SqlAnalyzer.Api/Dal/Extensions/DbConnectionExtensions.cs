using SqlAnalyzer.Api.Dal.Entities.DbConnection;

namespace SqlAnalyzer.Api.Dal.Extensions;

public static class DbConnectionExtensions
{
    public static string GetConnectionString(this DbConnection connection)
    {
        return $"Host={connection.Host};" +
               $"Port={connection.Port};" +
               $"Database={connection.Database};" +
               $"Username={connection.Username};" +
               $"Password={connection.Password};" +
               $"Pooling=false;Timeout=5";
    }
}