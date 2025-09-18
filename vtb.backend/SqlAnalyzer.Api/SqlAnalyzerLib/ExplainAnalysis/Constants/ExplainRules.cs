#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SqlAnalyzerLib.ExplainAnalysis.Enums;


/// <summary>
/// Типы проблем, выявляемых при анализе плана выполнения (EXPLAIN JSON)
/// </summary>
public enum ExplainRules
{
    SeqScanOnLargeTable,
    NestedLoopOnLargeTables,
    MisestimatedRows,
    HashAggOnLargeTable,
    FunctionScan,
    MaterializeNode,
    UnexpectedParallelism,
    CardinalityMismatch,
    BitmapHeapOverfetch,
    HashSpill,
    IndexFilterMismatch,
    IndexOnlyHeapFetch,
    NestedLoopHeavyInner,
    Parallelism,
    SeqScanSelective,
    SortExternal,
    TempFileSortSpill,
    HighBufferReads,
    LargeNumberOfLoops,
    RepeatedSeqScan,
    IndexOnlyScanButBitmap,
    HashJoinWithSkew,
    ParallelSeqScanIneffective,
    SortSpillToDisk,
    ExcessiveTempFiles,
    FunctionInWherePerformance,
    LeadingWildcardLike,
    MissingStatistics,
    CorrelatedSubqueryExec,
    SlowStartupTime,
    ActualVsEstimatedLargeDiff,
    FilterAfterAggregate,
    WorkMemExceededEstimate,
    LargeAggregateMemory,
    SortMethodExternal,
    BitmapIndexScanOnSmallTable,
    IndexScanWithFilterOnNonIndexedCol,
    SeqScanOnRecentlyUpdatedTable,
    SeqScanWithHighTempWrites,
    IndexScanButBitmapRecheck,
    ParallelWorkersTooMany,
    HashAggWithoutHashableKey,
    CrossProductDetected
}