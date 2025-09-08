using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

/// <summary>
/// контроллер для отслеживания состояния кеша 
/// </summary>
[ApiController]
[Route("api/cache")]
public class CacheAnalysisController : ControllerBase
{
    private readonly ICacheAnalyzeService _cacheAnalysisService;
    private readonly ILogger<CacheAnalysisController> _logger;

    public CacheAnalysisController(ICacheAnalyzeService cacheAnalysisService, ILogger<CacheAnalysisController> logger)
    {
        _cacheAnalysisService = cacheAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Анализ изменения состояния кеша за последний час
    /// </summary>
    /// <returns></returns>
    [HttpGet("analysis")]
    public async Task<ActionResult<CacheAnalysisResponse>> GetCacheAnalysis([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Запрос полного анализа кэша");
            var result = await _cacheAnalysisService.AnalyzeCacheAsync(dbConnectionId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе кэша");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
}