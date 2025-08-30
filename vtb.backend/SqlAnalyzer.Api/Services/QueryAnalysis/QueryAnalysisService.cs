using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

namespace SqlAnalyzer.Api.Services.QueryAnalysis;

using Dal.Entities.QueryAnalysis;

public class QueryAnalysisService : IQueryAnalysisService
{
    private readonly DataContext _db;

    public QueryAnalysisService(DataContext db)
    {
        _db = db;
    }

    public async Task<QueryAnalysisResultDto> AnalyzeAsync(QueryAnalysisDto request)
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

        _db.QueryAnalyzers.Add(analysis);
        await _db.SaveChangesAsync();

        return new QueryAnalysisResultDto(
            analysis.Id,
            analysis.DbConnectionId,
            analysis.Query,
            analysis.AnalyzeResult,
            analysis.CreateAt
        );
    }
}