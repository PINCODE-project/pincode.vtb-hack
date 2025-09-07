using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

/// <summary>
/// Контракт для статического SQL-анализатора.
/// </summary>
public interface IStaticSqlAnalyzer
{
    Task<StaticAnalysisResult> AnalyzeAsync(SqlQuery query, CancellationToken ct = default);
}