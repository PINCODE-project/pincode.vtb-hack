using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface ICacheAnalyzeService
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<CacheAnalysisResponse> AnalyzeCacheAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd);
}