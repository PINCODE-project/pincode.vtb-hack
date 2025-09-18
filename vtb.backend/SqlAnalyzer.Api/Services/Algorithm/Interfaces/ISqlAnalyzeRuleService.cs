using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.SqlAnalyzeRule;

namespace SqlAnalyzer.Api.Services.Algorithm.Interfaces;

public interface ISqlAnalyzeRuleService
{
    Task<IReadOnlyCollection<SqlAnalyzeRuleDto>> Find(int? skip, int? take);
    
    Task<Guid> Create(SqlAnalyzeRuleCreateDto dto);

    Task Update(SqlAnalyzeRuleUpdateDto dto);

    Task Delete(Guid id);

    Task<IReadOnlyCollection<Guid>> ApplyForQuery(Guid queryId, params IReadOnlyCollection<Guid> ruleIds);


}