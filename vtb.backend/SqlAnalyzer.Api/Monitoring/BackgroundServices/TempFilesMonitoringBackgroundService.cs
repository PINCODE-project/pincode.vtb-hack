using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Фоновый сервис для мониторинга состояния бд
/// </summary>
public class TempFilesMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<TempFilesMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Оптимальный интервал

    public TempFilesMonitoringBackgroundService(
        ILogger<TempFilesMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга временных файлов запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IMonitoringService>();
                _logger.LogInformation("Сбор метрик временных файлов...");
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var connectionStringList = await dbContext.DbConnections.ToListAsync();
                // TODO можно параллельно сделать
                foreach (var connection in connectionStringList)
                {
                    var connectionString = DbConnectionService.GetConnectionString(connection);
                    var success = await monitoringService.SaveTempFilesMetricsAsync(connectionString);
                    
                    if (success)
                    {
                        _logger.LogInformation("Метрики успешно сохранены {connectionString}", connectionString);
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось сохранить метрики{connectionString}", connectionString);
                    }    
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