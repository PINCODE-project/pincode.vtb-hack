using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzerLib.Recommendation.Models;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryService
{
    Task<Guid> Create(QueryCreateDto dto);

    Task<QueryDto> Get(Guid id);
    
    Task<IReadOnlyCollection<Recommendation>> AnalyzeAsync(Guid queryId, bool useLlm);
}