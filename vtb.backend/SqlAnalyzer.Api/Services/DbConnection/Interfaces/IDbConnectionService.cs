using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;

namespace SqlAnalyzer.Api.Services.DbConnection.Interfaces;

public interface IDbConnectionService
{
    Task<IReadOnlyCollection<DbConnectionDto>> Find(DbConnectionFindDto dto);
    Task<SimpleDto<Guid>> SaveAsync(DbConnectionCreateDto request);
    
    Task Update(DbConnectionUpdateDto dto);

    Task Delete(Guid Id);
    
    Task<DbConnectionCheckDto> CheckAsync(Guid dbConnectionId);
}