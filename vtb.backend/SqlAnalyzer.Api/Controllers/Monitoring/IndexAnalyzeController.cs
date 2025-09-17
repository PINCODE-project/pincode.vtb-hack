using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

[ApiController]
[Route("api/index")]
public class IndexAnalyzeController  : ControllerBase
{
    private readonly IIndexAnalysisService _analysisService;
    private readonly DataContext _dataContext;

    public IndexAnalyzeController(IIndexAnalysisService analysisService,
        DataContext dataContext)
    {
        _analysisService = analysisService;
        _dataContext = dataContext;
    }

    [HttpGet("recommendations")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IndexAnalysisResult))]
    public async Task<ActionResult<IndexAnalysisResult>> GetRecommendationsAsync([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var recommendations = await _analysisService.GetFullAnalysisAsync(dbConnectionId, startDate, endDate);
        return Ok(recommendations);
    }
    
    /// <summary>
    /// Получение метрик для отображения графиков по периоду
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType<List<IndexMetric>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricsForPeriodAsync([FromQuery] Guid dbConnectionId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate,
        [FromQuery] string? schemaName,
        [FromQuery] string? tableName)
    {
        var result = _dataContext.IndexMetrics
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
        
        return Ok(await result.ToListAsync());
    }
    
    /// <summary>
    /// Получение всех уникальных комбинаций схема-таблица
    /// </summary>
    [HttpGet("all-schema-and-table-name")]
    [ProducesResponseType<List<SchemaTableDto>>(StatusCodes.Status200OK)]
    public async Task<List<SchemaTableDto>> GetUniqueSchemaTableCombinationsAsync(Guid? dbConnectionId = null)
    {
        var query = _dataContext.IndexMetrics.AsQueryable();

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
    
    /// <summary>
    /// Получение уникального времени в записях
    /// </summary>
    [HttpGet("time")]
    [ProducesResponseType<List<DateTime>>(StatusCodes.Status200OK)]
    public async Task<List<DateTime>> GetUniqueTimeAsync(Guid? dbConnectionId = null)
    {
        var query = _dataContext.IndexMetrics.AsQueryable();

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