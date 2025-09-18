using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzer.Api.Dal.ValueObjects;

public class SqlAlgorithmAnalysisResult
{
    public StaticAnalysisResult QueryAnalysisResult { get; init; }
    public ExplainAnalysisResult? ExplainAnalysisResult { get; init; }
}