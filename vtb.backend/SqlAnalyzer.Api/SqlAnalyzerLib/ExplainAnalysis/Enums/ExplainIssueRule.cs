namespace SqlAnalyzerLib.ExplainAnalysis.Enums;

/// <summary>
/// Типы проблем, выявляемых при анализе плана выполнения (EXPLAIN JSON)
/// </summary>
public enum ExplainIssueRule
{
    SeqScanOnLargeTable,
    NestedLoopOnLargeTables,
    MissingIndex,
    MisestimatedRows,
    SortWithoutIndex,
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