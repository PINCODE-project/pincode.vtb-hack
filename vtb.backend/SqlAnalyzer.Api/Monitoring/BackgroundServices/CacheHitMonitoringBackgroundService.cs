using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Фоновый сервис для мониторинга состояния кеша бд
/// </summary>
public class CacheHitMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<CacheHitMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5); 

    public CacheHitMonitoringBackgroundService(
        ILogger<CacheHitMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга cache hit ratio запущен");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IMonitoringService>();
                _logger.LogInformation("Сбор метрик cache hit ratio...");
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var connectionStringList = await dbContext.DbConnections.ToListAsync(cancellationToken: stoppingToken);
                // TODO можно параллельно сделать
                foreach (var connection in connectionStringList)
                {
                    var success = await monitoringService.SaveCacheHitMetricsAsync(connection);

                    if (success)
                    {
                        _logger.LogInformation("Метрики cache hit ratio успешно сохранены, dbConnectionId={dbConnectionId}", connection.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось сохранить метрики cache hit ratio, dbConnectionId={dbConnectionId}", connection.Id);
                    }
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