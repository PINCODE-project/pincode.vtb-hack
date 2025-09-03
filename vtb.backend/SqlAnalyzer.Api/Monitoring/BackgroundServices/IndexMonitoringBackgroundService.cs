using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.BackgroundServices;

/// <summary>
/// Фоновый сервис для мониторинга состояния бд
/// </summary>
public class IndexMonitoringBackgroundService : BackgroundService
{
    private readonly ILogger<IndexMonitoringBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Оптимальный интервал

    public IndexMonitoringBackgroundService(
        ILogger<IndexMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис мониторинга индексов запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IIndexMonitoringService>();
                _logger.LogInformation("Сбор метрик индексов...");
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var connectionStringList = await dbContext.DbConnections.ToListAsync(cancellationToken: stoppingToken);
                foreach (var connection in connectionStringList)
                {
                    var connectionString = DbConnectionService.GetConnectionString(connection);
                    await monitoringService.CollectIndexStatisticsAsync(connectionString);
                    _logger.LogInformation("Метрики индексов успешно сохранены, connectionString={connectionString}", connectionString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом сервисе индексов");
            }

            // Ожидаем следующий интервал
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Сервис мониторинга индексов остановлен");
    }
}