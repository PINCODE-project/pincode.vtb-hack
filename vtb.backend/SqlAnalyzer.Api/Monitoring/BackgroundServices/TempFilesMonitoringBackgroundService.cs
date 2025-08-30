using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Фоновый сервис для мониторинга состояния бд
/// </summary>
public class TempFilesMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<TempFilesMonitoringBackgroundService> _logger;
    private readonly IMonitoringService _monitoringService;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Оптимальный интервал

    public TempFilesMonitoringBackgroundService(
        ILogger<TempFilesMonitoringBackgroundService> logger//,
        /*IMonitoringService monitoringService*/)
    {
        _logger = logger;
        //_monitoringService = monitoringService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга временных файлов запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Сбор метрик временных файлов...");
                bool success = await _monitoringService.SaveTempFilesMetricsAsync();
                    
                if (success)
                {
                    _logger.LogInformation("Метрики успешно сохранены");
                }
                else
                {
                    _logger.LogWarning("Не удалось сохранить метрики");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом сервисе");
            }

            // Ожидаем следующий интервал
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Сервис мониторинга временных файлов остановлен");
    }
}