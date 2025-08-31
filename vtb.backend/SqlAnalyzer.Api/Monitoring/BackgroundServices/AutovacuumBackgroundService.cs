using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

public class AutovacuumBackgroundService : BackgroundService
{
    private readonly ILogger<CacheHitMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5); 

    public AutovacuumBackgroundService(
        ILogger<CacheHitMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга autovacuum запущен");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IAutovacuumMonitoringService>();
                _logger.LogInformation("Старт сбора метрик autovacuum...");
                var success = await monitoringService.SaveAutovacuumMetricsAsync();
                    
                if (success)
                {
                    _logger.LogInformation("Метрики autovacuum успешно сохранены");
                }
                else
                {
                    _logger.LogWarning("Не удалось сохранить метрики autovacuum");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом сервисе autovacuum");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Сервис мониторинга autovacuum остановлен");
    }
}