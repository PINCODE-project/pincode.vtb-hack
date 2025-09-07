namespace SqlAnalyzerLib.Facade.Interfaces;

using Recommendation.Models;
public interface ISqlAnalyzerFacade
{
    Task<IReadOnlyCollection<Recommendation>> GetRecommendations(string query, string explainResult);
}