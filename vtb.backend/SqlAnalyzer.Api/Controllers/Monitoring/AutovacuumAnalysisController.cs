using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
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

    public AutovacuumAnalysisController(
        IAutovacuumAnalysisService analysisService,
        ILogger<AutovacuumAnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
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
}