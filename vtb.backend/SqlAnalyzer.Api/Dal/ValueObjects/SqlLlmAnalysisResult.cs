using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Dal.ValueObjects;

public class SqlLlmAnalysisResult
{
    public LlmAnswer LlmAnswer { get; set; }
    public ExplainRootPlan? ExplainResult { get; set; }
}