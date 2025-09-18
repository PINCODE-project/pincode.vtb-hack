namespace SqlAnalyzerLib.LibExtensions;

public class SqlAnalyzerLibOptions
{
    public IReadOnlyCollection<Type> SqlStaticAnalysisRules { get; set; } = new List<Type>();

    public IReadOnlyCollection<Type> ExplainAnalysisRules { get; set; } = new List<Type>();
}