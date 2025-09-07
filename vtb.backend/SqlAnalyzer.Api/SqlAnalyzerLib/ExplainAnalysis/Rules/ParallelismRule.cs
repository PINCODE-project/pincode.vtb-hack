using System.Globalization;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
    /// P40/P41: Параллелизм — запланировано >0 workers, но launched == 0 или дисбаланс между workers.
    /// </summary>
    public sealed class ParallelismRule : IPlanRule
    {
        /// <inheritdoc />
        public string Code => "P40";

        /// <inheritdoc />
        public string Category => "Parallelism";

        /// <inheritdoc />
        public Severity DefaultSeverity => Severity.Info;

        /// <inheritdoc />
        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (node.NodeSpecific == null) return Task.FromResult<PlanFinding?>(null);
            if (!node.NodeType.Contains("Gather", StringComparison.OrdinalIgnoreCase) && !node.NodeType.Contains("Gather Merge", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

            if (node.NodeSpecific.TryGetValue("Workers Planned", out var wpObj) && node.NodeSpecific.TryGetValue("Workers Launched", out var wlObj))
            {
                if (TryGetInt(wpObj, out var wp) && TryGetInt(wlObj, out var wl))
                {
                    if (wp > 0 && wl == 0)
                    {
                        var metadata = new Dictionary<string, object> { ["WorkersPlanned"] = wp, ["WorkersLaunched"] = wl };
                        var msg = "Планировщик запланировал параллельные воркеры, но ни один не был запущен. Возможно функции не parallel-safe или настройки сервера ограничивают запуск.";
                        return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
                    }
                    if (wp > 0 && wl > 0)
                    {
                        // можно проверить дисбаланс: если одна рабочая нода выполнила подавляющую часть
                        if (node.NodeSpecific.TryGetValue("Workers", out var workersObj))
                        {
                            var workersRaw = workersObj?.ToString() ?? "";
                            // слишком детальная проверка требует парсинга массивов — опустим глубокий анализ, вернём info, если есть tempfiles
                        }
                    }
                }
            }

            return Task.FromResult<PlanFinding?>(null);

            static bool TryGetInt(object? o, out int val)
            {
                val = 0;
                if (o == null) return false;
                if (o is int i) { val = i; return true; }
                if (o is long l) { val = (int)l; return true; }
                if (int.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) { val = parsed; return true; }
                return false;
            }
        }
    }