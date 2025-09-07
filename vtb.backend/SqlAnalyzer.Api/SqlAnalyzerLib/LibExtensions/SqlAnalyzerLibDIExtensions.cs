using Microsoft.Extensions.DependencyInjection;
using SqlAnalyzerLib.ExplainAnalysis;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.Facade;
using SqlAnalyzerLib.Facade.Interfaces;
using SqlAnalyzerLib.Recommendation;
using SqlAnalyzerLib.Recommendation.Interfaces;
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
            optionsBuilder.WithAllSqlStaticAnalyzerRules().WithAllExplainAnalyzerRules().WithAllRecommendationProviders();
        }
        
        var options = optionsBuilder.Build();
        
        
        services.AddTransient<ISqlAnalyzerFacade, SqlAnalyzerFacade>();

        // static analyzer
        services.AddTransient<IStaticSqlAnalyzer, StaticSqlAnalyzer>();
        foreach (var staticRule in options.SqlStaticAnalysisRules)
        {
            services.AddTransient<IStaticRule>(_ => staticRule);
        }
        
        // explain analyzer
        services.AddTransient<IExplainAnalyzer, ExplainAnalyzer>();
        services.AddTransient<IExplainParser, ExplainJsonParser>();
        services.AddTransient<IRuleEngine, RuleEngine>();
        foreach (var explainAnalysisRule in options.ExplainAnalysisRules)
        {
            services.AddTransient<IPlanRule>(_ => explainAnalysisRule);
        }

        services.AddTransient<RecommendationEngine>();
        foreach (var recommendationProvider in options.RecommendationProviders)
        {
            services.AddTransient<IRecommendationProvider>(_ => recommendationProvider);
        }
        
        return services;
    }
}