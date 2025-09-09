using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
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

    public TempFilesAnalyzeMonitoringController(ITempFilesAnalyzeService analysisService, ILogger<ITempFilesAnalyzeService> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
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
}