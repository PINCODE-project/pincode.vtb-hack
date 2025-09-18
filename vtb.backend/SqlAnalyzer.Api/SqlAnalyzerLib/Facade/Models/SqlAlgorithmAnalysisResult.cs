using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.Facade.Models;

public class SqlAlgorithmAnalysisResult
{
    public StaticAnalysisResult QueryAnalysisResult { get; init; }
    public ExplainAnalysisResult? ExplainAnalysisResult { get; init; }
}