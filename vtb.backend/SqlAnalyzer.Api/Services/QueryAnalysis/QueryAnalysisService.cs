using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;
using SqlAnalyzer.Api.Services.Recomedation;
using SqlAnalyzer.Api.Services.Recomedation.Interfaces;
using SqlAnalyzerLib.Facade.Interfaces;
using SqlAnalyzerLib.Recommendation.Models;

namespace SqlAnalyzer.Api.Services.QueryAnalysis;

using Dal.Entities.QueryAnalysis;
using SqlAnalyzerLib.Recommendation.Models;

public class QueryAnalysisService : IQueryAnalysisService
{
    private readonly DataContext _db;
    private readonly ISqlAnalyzerFacade _analyzer;

    public QueryAnalysisService(DataContext db, ISqlAnalyzerFacade analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    public async Task<IReadOnlyCollection<Recommendation>> AnalyzeAsync(QueryAnalysisDto request)
    {
        var dbConnection = await _db
            .DbConnections.FirstOrDefaultAsync(x => x.Id == request.DbConnectionId);

        if (dbConnection == null)
        {
            throw new InvalidOperationException("DbConnection not found");
        }
        
        string analyzeResult;
        await using (var conn = new NpgsqlConnection(dbConnection.GetConnectionString()))
        {
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand($"EXPLAIN (FORMAT JSON) {request.Sql}", conn);
            var result = await cmd.ExecuteScalarAsync();

            analyzeResult = result switch
            {
                string str => str,
                string[] arr => string.Join(Environment.NewLine, arr),
                _ => throw new Exception("Unexpected EXPLAIN output")
            };
        }

        var analysis = new QueryAnalysis
        {
            DbConnectionId = request.DbConnectionId,
            Query = request.Sql,
            AnalyzeResult = analyzeResult
        };

        _db.QueryAnalysis.Add(analysis);
        await _db.SaveChangesAsync();

        var analysisResult = await _analyzer.GetRecommendations(analysis.Query, analysis.AnalyzeResult);
        return analysisResult;
    }
}