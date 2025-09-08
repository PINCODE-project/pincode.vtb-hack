using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;
using SqlAnalyzerLib.Facade.Interfaces;

namespace SqlAnalyzer.Api.Services.QueryAnalysis;

using Dal.Entities.QueryAnalysis;
using SqlAnalyzerLib.Recommendation.Models;

public class QueryService : IQueryService
{
    private readonly DataContext _db;
    private readonly ISqlAnalyzerFacade _analyzer;

    public QueryService(DataContext db, ISqlAnalyzerFacade analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    public async Task<Guid> Create(QueryCreateDto dto)
    {
        var dbConnection = await _db
            .DbConnections.FirstOrDefaultAsync(x => x.Id == dto.DbConnectionId);

        if (dbConnection == null)
        {
            throw new InvalidOperationException("DbConnection not found");
        }
        
        string analyzeResult;
        await using (var conn = new NpgsqlConnection(dbConnection.GetConnectionString()))
        {
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand($"EXPLAIN (FORMAT JSON) {dto.Sql}", conn);
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
            DbConnectionId = dto.DbConnectionId,
            Query = dto.Sql,
            AnalyzeResult = analyzeResult
        };

        _db.QueryAnalysis.Add(analysis);
        await _db.SaveChangesAsync();

        return analysis.Id;
    }

    public async Task<QueryDto> Get(Guid id)
    {
        var query = await _db.QueryAnalysis.FirstOrDefaultAsync(x => x.Id == id);

        if (query == null)
        {
            throw new InvalidOperationException("Query not found");
        }
        
        return new QueryDto(query.Id, query.Query, query.AnalyzeResult, query.DbConnectionId, query.CreateAt);
    }

    public async Task<IReadOnlyCollection<Recommendation>> AnalyzeAsync(Guid queryId, bool useLlm)
    {
        var query = await _db.QueryAnalysis.FirstOrDefaultAsync(x => x.Id == queryId);
        
        if (query == null)
        {
            throw new InvalidOperationException("Query not found");
        }
        var analysisResult = await _analyzer.GetRecommendations(query.Query, query.AnalyzeResult);
        return analysisResult;
    }
}