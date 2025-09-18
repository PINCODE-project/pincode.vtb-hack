using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

/// <summary>
/// Контроллер для работы с метриками работы автовакуума
/// </summary>
[ApiController]
[Route("api/lock")]
public class PgLockAnalysisController : ControllerBase
{
    private readonly ILockAnalyzer _analysisService;
    private readonly ILogger<AutovacuumAnalysisController> _logger;
    private readonly DataContext _dataContext;
    private readonly IMonitoringService _monitoringService;

    public PgLockAnalysisController(
        ILockAnalyzer analysisService,
        ILogger<AutovacuumAnalysisController> logger,
        DataContext dataContext,
        IMonitoringService monitoringService)
    {
        _analysisService = analysisService;
        _logger = logger;
        _dataContext = dataContext;
        _monitoringService = monitoringService;
    }

    /// <summary>
    /// Полный анализ состояния autovacuum
    /// </summary>
    [HttpGet("analysis")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LockAnalysisResult))]
    public async Task<ActionResult<LockAnalysisResult>> GetFullAnalysis([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Запрос полного анализа lock");
            var result = await _analysisService.AnalyzeLocksAsync(dbConnectionId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении полного анализа lock");
            return StatusCode(500, new { error = "Произошла ошибка", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Получение метрик для отображения графиков по периоду
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType<List<PgLock>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricsForPeriodAsync([FromQuery] Guid dbConnectionId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var result = await _dataContext.PgLocks
            .Where(x => x.DbConnectionId == dbConnectionId 
                        && x.CreateAt >= startDate 
                        && x.CreateAt <= endDate).ToListAsync();
        
        return Ok(result);
    }
    
    /// <summary>
    /// Получение уникального времени в записях
    /// </summary>
    [HttpGet("time")]
    [ProducesResponseType<List<DateTime>>(StatusCodes.Status200OK)]
    public async Task<List<DateTime>> GetUniqueTimeAsync(Guid? dbConnectionId = null)
    {
        var query = _dataContext.PgLocks.AsQueryable();

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
    
    /// <summary>
    /// Принудительно собрать метрики
    /// </summary>
    [HttpPost("collect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CollectAsync([FromBody] Guid dbConnectionId)
    {
        var dbConnection = await _dataContext.DbConnections.FirstOrDefaultAsync(x => x.Id == dbConnectionId);
        if (dbConnection == null)
        {
            return BadRequest("dbConnection not found");
        }

        await _monitoringService.CollectLockDataAsync(dbConnection);
        return Ok();
    }
}