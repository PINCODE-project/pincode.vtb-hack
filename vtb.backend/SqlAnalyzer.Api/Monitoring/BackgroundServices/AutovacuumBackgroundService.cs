using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

public class AutovacuumBackgroundService : BackgroundService
{
    private readonly ILogger<CacheHitMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(4); 

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
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var connectionStringList = await dbContext.DbConnections.ToListAsync(cancellationToken: stoppingToken);
                foreach (var connection in connectionStringList)
                {
                    var connectionString = DbConnectionService.GetConnectionString(connection);
                    var success = await monitoringService.SaveAutovacuumMetricsAsync(connectionString);

                    if (success)
                    {
                        _logger.LogInformation("Метрики autovacuum успешно сохранены, connectionString={connectionString}", connectionString);
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось сохранить метрики autovacuum, connectionString={connectionString}", connectionString);
                    }
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