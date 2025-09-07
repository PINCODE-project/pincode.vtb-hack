namespace SqlAnalyzerLib.Common;

/// <summary>
/// Степень критичности найденной проблемы при анализе SQL-запроса или EXPLAIN
/// </summary>
public enum AnalysisSeverity
{
    Info,
    Warning,
    Critical
}