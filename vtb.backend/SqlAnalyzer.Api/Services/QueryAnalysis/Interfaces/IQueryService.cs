using SqlAnalyzer.Api.Dto.QueryAnalysis;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryService
{
    Task<Guid> Create(QueryCreateDto dto);

    Task<QueryDto> Get(Guid id);
    
    Task<QueryAnalysisResultDto> AnalyzeAsync(Guid queryId, bool useLlm);
}