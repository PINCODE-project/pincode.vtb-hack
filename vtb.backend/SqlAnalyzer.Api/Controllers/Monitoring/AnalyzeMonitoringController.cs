using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

/// <summary>
/// контроллер для получения результатов анализа метрик бд
/// </summary>
public class AnalyzeMonitoringController : ControllerBase
{
    private readonly IAnalyzeService _analysisService;
    private readonly ILogger<IAnalyzeService> _logger;

    public AnalyzeMonitoringController(IAnalyzeService analysisService, ILogger<IAnalyzeService> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    [HttpGet("temp-file/last-hour")]
    public async Task<ActionResult<RecommendationResponse>> AnalyzeLastHour()
    {
        try
        {
            _logger.LogInformation("Запуск анализа данных за последний час");
            var result = await _analysisService.AnalyzeTempFilesLastHourAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе данных за последний час");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("temp-file/custom-period")]
    public async Task<ActionResult<RecommendationResponse>> AnalyzeCustomPeriod(
        [FromQuery] DateTime start, 
        [FromQuery] DateTime end)
    {
        if (end <= start)
        {
            return BadRequest("Конечная дата должна быть больше начальной");
        }

        if ((end - start) > TimeSpan.FromDays(7))
        {
            return BadRequest("Период анализа не может превышать 7 дней");
        }

        try
        {
            _logger.LogInformation($"Запуск анализа данных за период с {start} по {end}");
            // Здесь можно добавить логику для анализа произвольного периода
            var result = await _analysisService.AnalyzeTempFilesLastHourAsync(); // Заглушка
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе данных за произвольный период");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}