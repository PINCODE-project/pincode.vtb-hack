using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;
using SqlAnalyzer.Api.Dal;
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
    private readonly IAutovacuumMonitoringService _monitoringService;
    private readonly ILogger<AutovacuumAnalysisController> _logger;
    private readonly DataContext _dataContext;

    public AutovacuumAnalysisController(
        IAutovacuumAnalysisService analysisService,
        IAutovacuumMonitoringService monitoringService,
        ILogger<AutovacuumAnalysisController> logger,
        DataContext dataContext)
    {
        _analysisService = analysisService;
        _monitoringService = monitoringService;
        _logger = logger;
        _dataContext = dataContext;
    }

    /// <summary>
    /// Полный анализ состояния autovacuum
    /// </summary>
    [HttpGet("analysis")]
    public async Task<ActionResult<AutovacuumAnalysisResponse>> GetFullAnalysis()
    {
        try
        {
            _logger.LogInformation("Запрос полного анализа autovacuum");
            var result = await _analysisService.AnalyzeAutovacuumLastHourAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении полного анализа autovacuum");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Статус здоровья autovacuum
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> GetHealthStatus()
    {
        try
        {
            _logger.LogInformation("Запрос статуса здоровья autovacuum");
            var analysis = await _analysisService.AnalyzeAutovacuumLastHourAsync();
                
            return Ok(new
            {
                Status = analysis.OverallStatus,
                StatusMessage = GetStatusMessage(analysis.OverallStatus),
                analysis.MetricsSummary.SystemWideDeadTupleRatio,
                CriticalTablesCount = analysis.MetricsSummary.CriticalTables,
                ProblematicTablesCount = analysis.MetricsSummary.ProblematicTables,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса здоровья autovacuum");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Список проблемных таблиц
    /// </summary>
    [HttpGet("problematic-tables")]
    public async Task<ActionResult<List<ProblematicTable>>> GetProblematicTables(
        [FromQuery] string? priority = null,
        [FromQuery] decimal minRatio = 0)
    {
        try
        {
            _logger.LogInformation("Запрос проблемных таблиц autovacuum");
            var analysis = await _analysisService.AnalyzeAutovacuumLastHourAsync();                
            var tables = analysis.ProblematicTables.AsQueryable();
                
            if (!string.IsNullOrEmpty(priority))
            {
                tables = tables.Where(t => t.Priority == priority);
            }
                
            if (minRatio > 0)
            {
                tables = tables.Where(t => t.DeadTupleRatio >= minRatio);
            }

            return Ok(tables.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении проблемных таблиц");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Рекомендации по настройке autovacuum
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<List<AutovacuumRecommendation>>> GetRecommendations(
        [FromQuery] string? severity = null,
        [FromQuery] string? type = null)
    {
        try
        {
            _logger.LogInformation("Запрос рекомендаций по autovacuum");
            var analysis = await _analysisService.AnalyzeAutovacuumLastHourAsync();
                
            var recommendations = analysis.Recommendations.AsQueryable();
                
            if (!string.IsNullOrEmpty(severity))
            {
                recommendations = recommendations.Where(r => r.Severity == severity);
            }
                
            if (!string.IsNullOrEmpty(type))
            {
                recommendations = recommendations.Where(r => r.Type == type);
            }

            return Ok(recommendations.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Принудительный сбор метрик
    /// </summary>
    [HttpPost("collect-metrics")]
    public async Task<ActionResult> CollectMetricsNow()
    {
        try
        {
            _logger.LogInformation("Принудительный сбор метрик autovacuum");
            var success = await _monitoringService.SaveAutovacuumMetricsAsync();
                
            return Ok(new
            {
                Success = success,
                Message = success ? "Метрики успешно собраны" : "Ошибка при сборе метрик",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при принудительном сборе метрик");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// История рекомендаций
    /// </summary>
    /// <remarks>Пока не сделано</remarks>
    [HttpGet("recommendations/history")]
    public async Task<ActionResult> GetRecommendationsHistory(
        [FromQuery] int days = 7,
        [FromQuery] string? severity = null)
    {
        try
        {
            _logger.LogInformation("Запрос истории рекомендаций за {Days} дней", days);
            var dateTime = DateTime.UtcNow.AddDays((-1) * days);
            var responseList = await _dataContext.AutovacuumStats.Where(r => r.CreateAt >= dateTime).ToListAsync();
            return Ok(new
            {
                responseList = responseList,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении истории рекомендаций");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    private string GetStatusMessage(string status)
    {
        return status switch
        {
            "healthy" => "Autovacuum работает оптимально",
            "warning" => "Требуется внимание к настройкам autovacuum",
            "critical" => "Критическое состояние! Требуется немедленное вмешательство",
            "attention" => "Необходим мониторинг и возможная корректировка",
            _ => "Неизвестный статус"
        };
    }
}