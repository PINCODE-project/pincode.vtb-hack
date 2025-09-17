using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

/// <summary>
/// контроллер для получения результатов анализа метрик бд
/// </summary>
[Route("temp-file")]
public class TempFilesAnalyzeMonitoringController : ControllerBase
{
    private readonly ITempFilesAnalyzeService _analysisService;
    private readonly ILogger<ITempFilesAnalyzeService> _logger;
    private readonly DataContext _dataContext;

    public TempFilesAnalyzeMonitoringController(ITempFilesAnalyzeService analysisService,
        ILogger<ITempFilesAnalyzeService> logger,
        DataContext dataContext)
    {
        _analysisService = analysisService;
        _logger = logger;
        _dataContext = dataContext;
    }

    /// <summary>
    /// Получить результаты анализа временных файлов за последний час
    /// </summary>
    /// <returns></returns>
    [HttpGet("analysis")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TempFilesRecommendationResponse))]
    public async Task<ActionResult<TempFilesRecommendationResponse>> AnalyzeLastHour([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Запуск анализа данных за последний час");
            var result = await _analysisService.AnalyzeTempFilesAsync(dbConnectionId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе данных за последний час");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Получение метрик для отображения графиков по периоду
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType<List<TempFilesStatsDal>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricsForPeriodAsync([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _dataContext.TempFilesStats
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
        var query = _dataContext.TempFilesStats.AsQueryable();

        if (dbConnectionId.HasValue)
        {
            query = query.Where(a => a.DbConnectionId == dbConnectionId.Value);
        }

        return await query
            .Select(a => a.CreateAt)
            .Distinct()
            .ToListAsync();
    }
}