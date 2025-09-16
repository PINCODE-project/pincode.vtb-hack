using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

/// <summary>
/// Контроллер для работы с метриками работы автовакуума
/// </summary>
[ApiController]
[Route("api/autovacuum")]
public class AutovacuumAnalysisController : ControllerBase
{
    private readonly IAutovacuumAnalysisService _analysisService;
    private readonly ILogger<AutovacuumAnalysisController> _logger;
    private readonly DataContext _dataContext;

    public AutovacuumAnalysisController(
        IAutovacuumAnalysisService analysisService,
        ILogger<AutovacuumAnalysisController> logger,
        DataContext dataContext)
    {
        _analysisService = analysisService;
        _logger = logger;
        _dataContext = dataContext;
    }

    /// <summary>
    /// Полный анализ состояния autovacuum
    /// </summary>
    [HttpGet("analysis")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AutovacuumAnalysisResponse))]
    public async Task<ActionResult<AutovacuumAnalysisResponse>> GetFullAnalysis([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Запрос полного анализа autovacuum");
            var result = await _analysisService.AnalyzeAutovacuumLastHourAsync(dbConnectionId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении полного анализа autovacuum");
            return StatusCode(500, new { error = "Произошла ошибка", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Получение метрик для отображения графиков по периоду
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType<List<AutovacuumStat>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricsForPeriodAsync([FromQuery] Guid dbConnectionId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate,
        [FromQuery] string? schemaName,
        [FromQuery] string? tableName)
    {
        var result = _dataContext.AutovacuumStats
            .Where(x => x.DbConnectionId == dbConnectionId 
                        && x.CreateAt >= startDate 
                        && x.CreateAt <= endDate);
        
        if (schemaName != null)
        {
            result = result.Where(t => t.SchemaName == schemaName);
        }
        
        if (tableName != null)
        {
            result = result.Where(t => t.TableName == tableName);
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Получение всех уникальных комбинаций схема-таблица
    /// </summary>
    [HttpGet("all-schema-and-table-name")]
    [ProducesResponseType<List<SchemaTableDto>>(StatusCodes.Status200OK)]
    public async Task<List<SchemaTableDto>> GetUniqueSchemaTableCombinationsAsync(Guid? dbConnectionId = null)
    {
        var query = _dataContext.AutovacuumStats.AsQueryable();

        if (dbConnectionId.HasValue)
        {
            query = query.Where(a => a.DbConnectionId == dbConnectionId.Value);
        }

        return await query
            .Select(a => new SchemaTableDto 
            { 
                SchemaName = a.SchemaName, 
                TableName = a.TableName 
            })
            .Distinct()
            .OrderBy(x => x.SchemaName)
            .ThenBy(x => x.TableName)
            .ToListAsync();
    }
}