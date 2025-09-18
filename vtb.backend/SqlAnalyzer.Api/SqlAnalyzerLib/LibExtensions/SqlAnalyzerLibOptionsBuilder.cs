using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

namespace SqlAnalyzerLib.LibExtensions;

public class SqlAnalyzerLibOptionsBuilder
{
    private readonly SqlAnalyzerLibOptions _options = new();

    public SqlAnalyzerLibOptionsBuilder WithAllSqlStaticAnalyzerRules()
    {
        var ruleType = typeof(IStaticRule);
        var staticRules = ruleType.Assembly.GetTypes().Where(t => t.IsClass && ruleType.IsAssignableFrom(t));
        _options.SqlStaticAnalysisRules = staticRules.ToList();

        return this;
    }
    
    public SqlAnalyzerLibOptionsBuilder WithAllExplainAnalyzerRules()
    {
        var ruleType = typeof(IPlanRule);
        var planRules = ruleType.Assembly.GetTypes().Where(t => t.IsClass && ruleType.IsAssignableFrom(t));
        _options.ExplainAnalysisRules = planRules.ToList();

        return this;
    }

    public SqlAnalyzerLibOptions Build()
    {
        return _options;
    }
}