using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Base;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.Algorithm.Interfaces;
using SqlAnalyzer.Api.Services.LlmClient.Interfaces;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;
using SqlAnalyzerLib.Facade.Interfaces;

namespace SqlAnalyzer.Api.Services.QueryAnalysis;

using Dal.Entities.QueryAnalysis;

/// <inheritdoc />
public class QueryService : IQueryService
{
    private readonly DataContext _db;
    private readonly ISqlAnalyzerFacade _analyzer;
    private readonly ILlmClient _llm;
    private readonly ISqlAnalyzeRuleService _customRulesService;
    private readonly ILogger<IQueryService> _logger;

    /// 
    public QueryService(DataContext db,
        ISqlAnalyzerFacade analyzer,
        ILlmClient llm,
        ISqlAnalyzeRuleService customRulesService,
        ILogger<IQueryService> logger)
    {
        _db = db;
        _analyzer = analyzer;
        _llm = llm;
        _customRulesService = customRulesService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<QueryDto>> Find(QueriesFindDto dto)
    {
        var queries = await _db
            .Queries.AsNoTracking()
            .UseLimiter(dto.Skip, dto.Take)
            .Select(query => new QueryDto
            {
                Id = query.Id,
                Sql = query.Sql,
                ExplainResult = query.ExplainResult ?? "",
                DbConnectionId = query.DbConnectionId,
                CreatedAt = query.CreateAt
            })
            .ToListAsync();
        return queries;
    }

    /// <inheritdoc />
    public async Task<Guid> Create(QueryCreateDto dto)
    {
        var dbConnection = await _db
            .DbConnections.FirstOrDefaultAsync(x => x.Id == dto.DbConnectionId);

        if (dbConnection == null)
        {
            throw new InvalidOperationException("DbConnection not found");
        }

        var analyzeResult = string.Empty;
        try
        {
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
        }
        catch
        {
            _logger.LogError("Failed to execute explain");
        }

        var analysis = new QueryAnalysis
        {
            DbConnectionId = dto.DbConnectionId,
            Sql = dto.Sql,
            ExplainResult = analyzeResult
        };

        _db.Queries.Add(analysis);
        await _db.SaveChangesAsync();

        return analysis.Id;
    }

    /// <inheritdoc />
    public async Task<QueryDto> Get(Guid id)
    {
        var query = await _db.Queries.FirstOrDefaultAsync(x => x.Id == id);

        if (query == null)
        {
            throw new InvalidOperationException("Query not found");
        }

        return new QueryDto
        {
            Id = query.Id,
            Sql = query.Sql,
            ExplainResult = query.ExplainResult ?? "",
            DbConnectionId = query.DbConnectionId,
            CreatedAt = query.CreateAt
        };
    }

    /// <inheritdoc />
    public async Task<QueryAnalysisResultDto> Analyze(Guid queryId, bool useLlm, IReadOnlyCollection<Guid>? ruleIds)
    {
        var queryAnalysisResult = await _db
            .QueryAnalysisResults.Where(x => x.QueryId == queryId)
            .Select(x => new
            {
                x.Query,
                AnalysisResult = x
            }).FirstOrDefaultAsync();

        if (queryAnalysisResult?.AnalysisResult is not null)
        {
            await UpdateQueryAnalysisResult(useLlm, ruleIds, queryAnalysisResult.AnalysisResult);

            return new QueryAnalysisResultDto
            {
                Id = queryAnalysisResult.Query.Id,
                DbConnectionId = queryAnalysisResult.Query.DbConnectionId,
                Query = queryAnalysisResult.Query.Sql,
                ExplainResult = queryAnalysisResult.Query.ExplainResult ?? string.Empty,
                AlgorithmRecommendation = queryAnalysisResult.AnalysisResult.Recommendations,
                LlmRecommendations = queryAnalysisResult.AnalysisResult.LlmRecommendations,
                FindindCustomRules = queryAnalysisResult.AnalysisResult.FindindCustomRules ?? []
            };
        }

        var query = await _db.Queries.FirstOrDefaultAsync(x => x.Id == queryId);
        if (query == null)
        {
            throw new InvalidOperationException("Query not found");
        }

        var analysisResult = await _analyzer.GetRecommendations(query.Sql, query.ExplainResult ?? string.Empty);
        var llmAnswer = useLlm
            ? await _llm.GetRecommendation(
                query.Sql,
                query.ExplainResult
            )
            : null;

        var customFindings = await _customRulesService.ApplyForQuery(query, ruleIds ?? []);
        var result = new QueryAnalysisResult
        {
            QueryId = query.Id,
            Recommendations = analysisResult,
            LlmRecommendations = llmAnswer,
            FindindCustomRules = customFindings.ToList()
        };

        _db.QueryAnalysisResults.Add(result);
        await _db.SaveChangesAsync();


        return new QueryAnalysisResultDto
        {
            Id = query.Id,
            DbConnectionId = query.DbConnectionId,
            Query = query.Sql,
            ExplainResult = query.ExplainResult ?? string.Empty,
            AlgorithmRecommendation = analysisResult,
            LlmRecommendations = llmAnswer,
        };
    }

    private async Task UpdateQueryAnalysisResult(bool useLlm, IReadOnlyCollection<Guid>? ruleIds, QueryAnalysisResult analysisResult)
    {
        var isUpdated = false;
        if (useLlm && analysisResult.LlmRecommendations == null)
        {
            var llmRecommendations = await _llm.GetRecommendation(
                analysisResult.Query.Sql,
                analysisResult.Query.ExplainResult);
            analysisResult.LlmRecommendations = llmRecommendations;
            isUpdated = true;
        }

        var newRules = ruleIds?.Except(analysisResult.FindindCustomRules ?? []).ToList();
        if (newRules?.Count > 0)
        {
            var newFindings = await _customRulesService.ApplyForQuery(analysisResult.Query, newRules);
            if (analysisResult.FindindCustomRules is null)
            {
                analysisResult.FindindCustomRules = newFindings.ToList();
            }
            else
            {
                analysisResult.FindindCustomRules.AddRange(newFindings);
            }

            isUpdated = true;
        }

        if (isUpdated)
        {
            await _db.SaveChangesAsync();
        }
    }
}