using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.PgStateAnalysis;

[ApiController]
[Route("api/pg-state-analysis")]
public class PgStateAnalysisController : ControllerBase
{
    private readonly IPgStatAnalyzerService _analyzer;

    public PgStateAnalysisController(IPgStatAnalyzerService analyzer)
    {
        _analyzer = analyzer;
    }

    /// <summary>
    /// GET /api/pganalysis/top?limit=50&orderBy=total_exec_time
    /// Возвращает анализ топ-N запросов из pg_stat_statements.
    /// </summary>
    [HttpGet("top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AnalysisReportAdvanced))]
    public async Task<IActionResult> GetTop([FromQuery] Guid dbConnectionId)
    {
        try
        {
            var report = await _analyzer.AnalyzeTopAsync(dbConnectionId);
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}