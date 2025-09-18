using SqlAnalyzer.Api.Dto.SqlAnalyzeRule;

namespace SqlAnalyzer.Api.Services.Algorithm.Interfaces;

using SqlAnalyzer.Api.Dal.Entities.QueryAnalysis;

public interface ISqlAnalyzeRuleService
{
    Task<IReadOnlyCollection<SqlAnalyzeRuleDto>> Find(IReadOnlyCollection<Guid>? ids, int? skip, int? take);
    
    Task<Guid> Create(SqlAnalyzeRuleCreateDto dto);

    Task Update(SqlAnalyzeRuleUpdateDto dto);

    Task Delete(Guid id);

    Task<IReadOnlyCollection<Guid>> ApplyForQuery(QueryAnalysis query, params IReadOnlyCollection<Guid> ruleIds);


}