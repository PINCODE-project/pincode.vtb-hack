using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// Результат одного статического правила.
/// </summary>
public record StaticAnalysisPoint(StaticRules RuleType, Severity Severity, string Problem, string Recommendations);