using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryExplainer
{
    Task<ExplainRootPlan?> Execute(string dbConnectionString, string query);
    PlanComparisonDto? Compare(ExplainRootPlan? oldPlan, ExplainRootPlan? newPlan);
}