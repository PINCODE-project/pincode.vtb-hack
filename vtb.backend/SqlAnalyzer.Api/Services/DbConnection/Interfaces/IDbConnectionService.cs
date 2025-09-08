using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;

namespace SqlAnalyzer.Api.Services.DbConnection.Interfaces;

public interface IDbConnectionService
{
    Task<IReadOnlyCollection<DbConnectionDto>> Find(DbConnectionFindDto dto);
    Task<SimpleDto<Guid>> SaveAsync(DbConnectionCreateDto dto);
    
    Task Update(DbConnectionUpdateDto dto);

    Task Delete(Guid id);
    
    Task<DbConnectionCheckDto> CheckAsync(DbConnectionCreateDto dto);
}