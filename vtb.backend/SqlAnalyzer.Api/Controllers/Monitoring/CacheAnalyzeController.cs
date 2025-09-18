using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
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
    private readonly DataContext _dataContext;

    public CacheAnalysisController(ICacheAnalyzeService cacheAnalysisService,
        ILogger<CacheAnalysisController> logger,
        DataContext dataContext)
    {
        _cacheAnalysisService = cacheAnalysisService;
        _logger = logger;
        _dataContext = dataContext;
    }

    /// <summary>
    /// Анализ изменения состояния кеша за последний час
    /// </summary>
    /// <returns></returns>
    [HttpGet("analysis")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CacheAnalysisResponse))]
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

    /// <summary>
    /// Получение метрик для отображения графиков по периоду
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType<List<CacheHitStats>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricsForPeriodAsync([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _dataContext.CacheHitStats
            .Where(x => x.DbConnectionId == dbConnectionId 
                && x.CreateAt >= startDate 
                && x.CreateAt <= endDate)
            .ToListAsync();
        return Ok(result);
    }
    
    /// <summary>
    /// Получение уникального времени в записях
    /// </summary>
    [HttpGet("time")]
    [ProducesResponseType<List<DateTime>>(StatusCodes.Status200OK)]
    public async Task<List<DateTime>> GetUniqueTimeAsync(Guid? dbConnectionId = null)
    {
        var query = _dataContext.CacheHitStats.AsQueryable();

        if (dbConnectionId.HasValue)
        {
            query = query.Where(a => a.DbConnectionId == dbConnectionId.Value);
        }

        return await query
            .Select(a => a.CreateAt)
            .Distinct()
            .OrderByDescending(date => date)
            .ToListAsync();
    }
}