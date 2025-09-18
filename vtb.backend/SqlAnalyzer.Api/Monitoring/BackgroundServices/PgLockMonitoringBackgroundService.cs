using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Мониторинг блокировок в постгресе
/// </summary>
public class PgLockMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<TempFilesMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Оптимальный интервал

    public PgLockMonitoringBackgroundService(
        ILogger<TempFilesMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга блокировок запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IMonitoringService>();
                _logger.LogInformation("Сбор метрик блокировок...");
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var connectionStringList = await dbContext.DbConnections.ToListAsync();
                // TODO можно параллельно сделать
                foreach (var connection in connectionStringList)
                {
                    await monitoringService.CollectLockDataAsync(connection);
                    _logger.LogInformation("Метрики успешно сохранены, dbConnectionId={dbConnectionId}", connection.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом сервисе сбора блокировок");
            }

            // Ожидаем следующий интервал
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Сервис мониторинга блокировок остановлен");
    }
}