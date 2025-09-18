using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Описание одного найденного "находки" (issue) по плану.
/// </summary>
public record PlanFinding(ExplainRules Code, Severity Severity, string Problem, string Recommendation);