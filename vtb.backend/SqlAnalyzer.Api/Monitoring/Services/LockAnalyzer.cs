using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Анализатор блокировок PostgreSQL
/// </summary>
public class LockAnalyzer : ILockAnalyzer
{
    private readonly DataContext _context;

    public LockAnalyzer(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Анализирует блокировки за указанный период и возвращает рекомендации
    /// </summary>
    public async Task<LockAnalysisResult> AnalyzeLocksAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd)
    {
        var recentLocks = await _context.PgLocks
            .Where(l => l.CreateAt >= periodStart 
                        && l.CreateAt <= periodEnd 
                        && l.DbConnectionId == dbConnectionId
                        && !l.Granted)
            .ToListAsync();

        var result = new LockAnalysisResult
        {
            TotalBlockedLocks = recentLocks.Count,
            BlockedProcesses = recentLocks.Select(l => l.Pid).Distinct().Count()
        };

        if (recentLocks.Count == 0)
        {
            return result;
        }

        AnalyzeCriticalIssues(recentLocks, result);
        AnalyzeWarningIssues(recentLocks, result);
        GenerateRecommendations(recentLocks, result);
        IdentifyTopProblems(recentLocks, result);

        return result;
    }

    /// <summary>
    /// Анализ критических проблем
    /// </summary>
    private void AnalyzeCriticalIssues(List<PgLock> locks, LockAnalysisResult result)
    {
        // Долгие AccessExclusiveLock блокировки (> 10 секунд)
        var longAelLocks = locks.Where(l => 
            l.Mode == "AccessExclusiveLock" && 
            l.WaitTimeMs > 10000);
        
        if (longAelLocks.Any())
        {
            result.CriticalIssues.Add($"AccessExclusiveLock блокировки > 10s: {longAelLocks.Count()}");
        }

        // Блокировки транзакций с долгим ожиданием
        var longTxLocks = locks.Where(l => 
            l.LockType == "transactionid" && 
            l.WaitTimeMs > 30000);
        
        if (longTxLocks.Any())
        {
            result.CriticalIssues.Add($"Блокировки транзакций > 30s: {longTxLocks.Count()}");
        }

        // Множественные блокировки на системных таблицах
        var systemTableLocks = locks.Where(l => 
            l.RelationOid < 16384); // OID системных таблиц
        
        if (systemTableLocks.Any())
        {
            result.CriticalIssues.Add($"Блокировки системных таблиц: {systemTableLocks.Count()}");
        }
    }

    /// <summary>
    /// Анализ проблемных ситуаций
    /// </summary>
    private void AnalyzeWarningIssues(List<PgLock> locks, LockAnalysisResult result)
    {
        // Множественные блокировки отношений
        var relationLocks = locks.Count(l => l.LockType == "relation");
        if (relationLocks > 5)
        {
            result.WarningIssues.Add($"Множественные блокировки таблиц: {relationLocks}");
        }

        // Блокировки кортежей
        var tupleLocks = locks.Count(l => l.LockType == "tuple");
        if (tupleLocks > 10)
        {
            result.WarningIssues.Add($"Блокировки строк: {tupleLocks}");
        }

        // Долгие запросы (> 5 секунд)
        var longQueries = locks.Count(l => l.WaitTimeMs > 5000);
        if (longQueries > 0)
        {
            result.WarningIssues.Add($"Долгие заблокированные запросы (>5s): {longQueries}");
        }

        // Частые блокировки одного процесса
        var frequentBlockers = locks
            .GroupBy(l => l.Pid)
            .Where(g => g.Count() > 3)
            .Select(g => g.Key);
        
        if (frequentBlockers.Any())
        {
            result.WarningIssues.Add($"Процессы с частыми блокировками: {string.Join(", ", frequentBlockers)}");
        }
    }

    /// <summary>
    /// Генерация рекомендаций
    /// </summary>
    private void GenerateRecommendations(List<PgLock> locks, LockAnalysisResult result)
    {
        var recommendations = new List<string>();

        // Рекомендации по типам блокировок
        if (locks.Any(l => l.LockType == "relation" && l.Mode.Contains("Exclusive")))
        {
            recommendations.Add("Оптимизировать DDL операции - выполнять в maintenance window");
        }

        if (locks.Any(l => l.LockType == "tuple"))
        {
            recommendations.Add("Добавить индексы для часто обновляемых таблиц");
            recommendations.Add("Уменьшить время транзакций - разбить на батчи");
        }

        if (locks.Any(l => l.LockType == "transactionid"))
        {
            recommendations.Add("Проверить порядок обновления строк в транзакциях");
            recommendations.Add("Использовать NOWAIT для не-critical операций");
        }

        // Рекомендации по времени ожидания
        if (locks.Any(l => l.WaitTimeMs > 10000))
        {
            recommendations.Add("Установить lock_timeout для долгих операций");
            recommendations.Add("Рассмотреть использование SKIP LOCKED для фоновых задач");
        }

        // Рекомендации по режимам блокировок
        if (locks.Any(l => l.Mode == "AccessExclusiveLock"))
        {
            recommendations.Add("Избегать DDL операций в рабочее время");
            recommendations.Add("Использовать CONCURRENTLY для создания индексов");
        }

        if (locks.Count > 20)
        {
            recommendations.Add("Пересмотреть уровень изоляции транзакций");
            recommendations.Add("Включить логирование медленных запросов");
        }

        result.Recommendations = recommendations.Distinct().ToList();
    }

    /// <summary>
    /// Идентификация топ проблем
    /// </summary>
    private void IdentifyTopProblems(List<PgLock> locks, LockAnalysisResult result)
    {
        result.TopBlockingPids = locks
            .GroupBy(l => l.Pid)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        // Для демонстрации - используем OID таблиц
        result.TopBlockedTables = locks
            .Where(l => l.RelationOid.HasValue)
            .GroupBy(l => l.RelationOid.Value)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => $"table_oid_{g.Key}")
            .ToList();
    }

    /// <summary>
    /// Получить детальную информацию по конкретному процессу
    /// </summary>
    public async Task<string> GetProcessDetailsAsync(int pid)
    {
        var processLocks = await _context.PgLocks
            .Where(l => l.Pid == pid && !l.Granted)
            .OrderByDescending(l => l.CreateAt)
            .Take(10)
            .ToListAsync();

        if (!processLocks.Any())
            return $"Процесс {pid} не имеет заблокированных lock'ов";

        return $"Процесс {pid}: {processLocks.Count} блокировок, " +
               $"последний запрос: {processLocks.First().Query?.Truncate(100)}";
    }
}

/// <summary>
/// Результат анализа блокировок с рекомендациями
/// </summary>
public class LockAnalysisResult
{
    /// <summary>
    /// Время проведения анализа
    /// </summary>
    public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Общее количество заблокированных lock'ов
    /// </summary>
    public int TotalBlockedLocks { get; set; }
    
    /// <summary>
    /// Количество уникальных заблокированных процессов
    /// </summary>
    public int BlockedProcesses { get; set; }
    
    /// <summary>
    /// Критические блокировки (требуют немедленного вмешательства)
    /// </summary>
    public List<string> CriticalIssues { get; set; } = new();
    
    /// <summary>
    /// Проблемные блокировки (требуют внимания)
    /// </summary>
    public List<string> WarningIssues { get; set; } = new();
    
    /// <summary>
    /// Рекомендации по устранению
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Наиболее проблемные PID процессов
    /// </summary>
    public List<int> TopBlockingPids { get; set; } = new();
    
    /// <summary>
    /// Наиболее блокируемые таблицы
    /// </summary>
    public List<string> TopBlockedTables { get; set; } = new();
}

public static class LockAnalysisExtensions
{
    /// <summary>
    /// Обрезает строку до указанной длины
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    /// <summary>
    /// Форматирует результат анализа для логов
    /// </summary>
    public static string ToLogString(this LockAnalysisResult result)
    {
        return $"Анализ блокировок: {result.TotalBlockedLocks} blocked, " +
               $"{result.CriticalIssues.Count} critical, " +
               $"{result.WarningIssues.Count} warnings";
    }
}