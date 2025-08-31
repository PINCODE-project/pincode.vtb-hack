using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

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

    [HttpGet("analysis/last-hour")]
    public async Task<ActionResult<CacheAnalysisResponse>> GetCacheAnalysis()
    {
        try
        {
            _logger.LogInformation("Запрос полного анализа кэша за последний час");
            var result = await _cacheAnalysisService.AnalyzeCacheLastHourAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе кэша");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public async Task<ActionResult<CacheHealthStatus>> GetCacheHealth()
    {
        try
        {
            _logger.LogInformation("Запрос статуса здоровья кэша");
            var result = await _cacheAnalysisService.GetCacheHealthStatusAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса здоровья кэша");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("metrics/summary")]
    public async Task<ActionResult<CacheMetricsSummary>> GetCacheMetricsSummary()
    {
        try
        {
            _logger.LogInformation("Запрос сводки метрик кэша");
            var analysis = await _cacheAnalysisService.AnalyzeCacheLastHourAsync();
            return Ok(analysis.MetricsSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении сводки метрик кэша");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<List<CacheRecommendation>>> GetCacheRecommendations()
    {
        try
        {
            _logger.LogInformation("Запрос рекомендаций по кэшу");
            var analysis = await _cacheAnalysisService.AnalyzeCacheLastHourAsync();
            return Ok(analysis.Recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций по кэшу");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}