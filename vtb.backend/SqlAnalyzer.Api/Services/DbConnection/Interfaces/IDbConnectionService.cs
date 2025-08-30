using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;

namespace SqlAnalyzer.Api.Services.DbConnection.Interfaces;

public interface IDbConnectionService
{
    Task<SimpleDto<Guid>> SaveAsync(DbConnectionCreateDto request);
    
    Task<DbConnectionCheckDto> CheckAsync(Guid dbConnectionId);
}