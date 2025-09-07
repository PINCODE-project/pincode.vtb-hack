using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.DbConnection;

public class DbConnection : EntityBase, IEntityCreatedAt
{
    public required string Name { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Database { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }  
    public DateTime CreateAt { get; set; }
}