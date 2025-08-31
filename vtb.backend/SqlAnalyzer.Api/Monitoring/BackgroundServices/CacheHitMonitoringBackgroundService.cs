using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Фоновый сервис для мониторинга состояния кеша бд
/// </summary>
public class CacheHitMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<CacheHitMonitoringBackgroundService> _logger;
    private readonly IMonitoringService _monitoringService;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5); // Такой же интервал

    public CacheHitMonitoringBackgroundService(
        ILogger<CacheHitMonitoringBackgroundService> logger,
        IMonitoringService monitoringService)
    {
        _logger = logger;
        _monitoringService = monitoringService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга cache hit ratio запущен");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Сбор метрик cache hit ratio...");
                bool success = await _monitoringService.SaveCacheHitMetricsAsync();
                    
                if (success)
                {
                    _logger.LogInformation("Метрики cache hit ratio успешно сохранены");
                }
                else
                {
                    _logger.LogWarning("Не удалось сохранить метрики cache hit ratio");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом сервисе cache hit ratio");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Сервис мониторинга cache hit ratio остановлен");
    }
}