using SqlAnalyzer.Api.Dto.QueryAnalysis;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryService
{
    Task<IReadOnlyCollection<QueryDto>> Find(QueriesFindDto dto);
    Task<Guid> Create(QueryCreateDto dto);

    Task<QueryDto> Get(Guid id);
    
    Task<QueryAnalysisResultDto> Analyze(Guid queryId, bool useLlm, IReadOnlyCollection<Guid>? ruleIds);
}