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
    UnexpectedParallelism
}