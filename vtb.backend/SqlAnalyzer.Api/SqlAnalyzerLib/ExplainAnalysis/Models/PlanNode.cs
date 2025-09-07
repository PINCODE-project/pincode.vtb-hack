namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
    /// Узел плана EXPLAIN. NodeSpecific содержит все детальные поля, которые не попали в стандартные свойства.
    /// </summary>
    public record PlanNode
    {
        /// <summary>
        /// Node Type, как указан в JSON (например, "Seq Scan", "Hash Join").
        /// </summary>
        public string NodeType { get; init; } = string.Empty;

        /// <summary>
        /// Маппинг NodeType на enum, если возможно.
        /// </summary>
        public string? ShortNodeType { get; init; }

        /// <summary>
        /// Оценочная startup cost.
        /// </summary>
        public double? StartupCost { get; init; }

        /// <summary>
        /// Оценочная total cost.
        /// </summary>
        public double? TotalCost { get; init; }

        /// <summary>
        /// Оценка планировщика: планируемое количество строк.
        /// </summary>
        public double? PlanRows { get; init; }

        /// <summary>
        /// Оценочная ширина строки (в байтах).
        /// </summary>
        public double? PlanWidth { get; init; }

        /// <summary>
        /// Фактическое стартовое время в миллисекундах (ANALYZE).
        /// </summary>
        public double? ActualStartupTimeMs { get; init; }

        /// <summary>
        /// Фактическое суммарное время в миллисекундах (ANALYZE).
        /// </summary>
        public double? ActualTotalTimeMs { get; init; }

        /// <summary>
        /// Фактическое количество строк (single loop).
        /// </summary>
        public double? ActualRows { get; init; }

        /// <summary>
        /// Количество выполнений узла (loops).
        /// </summary>
        public int? ActualLoops { get; init; }

        /// <summary>
        /// Блоки буферов (shared/local/temp) — если BUFFERS был включён.
        /// </summary>
        public BufferStats? Buffers { get; init; }

        /// <summary>
        /// Словарь с произвольными свойствами узла (Index Name, Index Cond, Sort Method, Batches, Disk Usage и т.д.).
        /// </summary>
        public IReadOnlyDictionary<string, object>? NodeSpecific { get; init; }

        /// <summary>
        /// Дочерние узлы плана.
        /// </summary>
        public IReadOnlyList<PlanNode>? Children { get; init; }
    }