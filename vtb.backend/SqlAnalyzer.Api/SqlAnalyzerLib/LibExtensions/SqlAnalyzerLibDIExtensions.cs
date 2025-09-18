using SqlAnalyzerLib.ExplainAnalysis;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.Facade;
using SqlAnalyzerLib.Facade.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

namespace SqlAnalyzerLib.LibExtensions;

/// <summary>
/// Класс для подключения либы в DI
/// </summary>
public static class SqlAnalyzerLibDIExtensions
{
    public static IServiceCollection AddSqlAnalyzer(this IServiceCollection services, Action<SqlAnalyzerLibOptionsBuilder>? customOptionBuilder = null)
    {
        var optionsBuilder = new SqlAnalyzerLibOptionsBuilder();
        if (customOptionBuilder is not null)
        {
            customOptionBuilder(optionsBuilder);
        }
        else
        {
            optionsBuilder.WithAllSqlStaticAnalyzerRules().WithAllExplainAnalyzerRules();
        }
        
        var options = optionsBuilder.Build();
        
        
        services.AddTransient<ISqlAnalyzerFacade, SqlAnalyzerFacade>();

        // static analyzer
        services.AddTransient<IStaticSqlAnalyzer, StaticSqlAnalyzer>();
        foreach (var staticRule in options.SqlStaticAnalysisRules)
        {
            var ruleInterfaceType = typeof(IStaticRule);
            services.Add(new ServiceDescriptor(ruleInterfaceType, staticRule, ServiceLifetime.Transient));
        }
        
        // explain analyzer
        services.AddTransient<IExplainAnalyzer, ExplainAnalyzer>();
        services.AddTransient<IExplainParser, ExplainJsonParser>();
        services.AddTransient<IRuleEngine, RuleEngine>();
        foreach (var explainAnalysisRule in options.ExplainAnalysisRules)
        {
            var ruleInterfaceType = typeof(IPlanRule);
            services.Add(new ServiceDescriptor(ruleInterfaceType, explainAnalysisRule, ServiceLifetime.Transient));
        }
        
        return services;
    }
}