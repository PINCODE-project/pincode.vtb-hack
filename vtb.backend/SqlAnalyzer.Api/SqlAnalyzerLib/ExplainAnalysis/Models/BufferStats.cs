namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Метрики буферов, агрегированные для узла.
/// </summary>
public record BufferStats
{
    /// <summary>
    /// Shared buffer hits.
    /// </summary>
    public long SharedHit { get; init; }

    /// <summary>
    /// Shared buffer reads (from disk).
    /// </summary>
    public long SharedRead { get; init; }

    /// <summary>
    /// Local buffer hits.
    /// </summary>
    public long LocalHit { get; init; }

    /// <summary>
    /// Local buffer reads.
    /// </summary>
    public long LocalRead { get; init; }

    /// <summary>
    /// Temp blocks read (from temp files).
    /// </summary>
    public long TempRead { get; init; }

    /// <summary>
    /// Temp blocks written (to temp files).
    /// </summary>
    public long TempWritten { get; init; }
}