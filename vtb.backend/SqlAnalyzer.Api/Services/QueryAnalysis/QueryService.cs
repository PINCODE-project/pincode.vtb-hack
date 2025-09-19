using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Base;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dal.ValueObjects;
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
    private readonly IQueryExplainer _explainer;


    /// 
    public QueryService(
        DataContext db,
        ISqlAnalyzerFacade analyzer,
        ILlmClient llm,
        ISqlAnalyzeRuleService customRulesService,
        IQueryExplainer explainer
    )
    {
        _db = db;
        _analyzer = analyzer;
        _llm = llm;
        _customRulesService = customRulesService;
        _explainer = explainer;
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
                ExplainResult = query.ExplainResult,
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

        var explainResult = await _explainer.Execute(dbConnection.GetConnectionString(), dto.Sql);
        var analysis = new QueryAnalysis
        {
            DbConnectionId = dto.DbConnectionId,
            Sql = dto.Sql,
            ExplainResult = explainResult
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
            ExplainResult = query.ExplainResult,
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

        var query = queryAnalysisResult?.Query;
        if (queryAnalysisResult?.AnalysisResult is { } analysis && query is not null)
        {
            await UpdateQueryAnalysisResult(useLlm, ruleIds, queryAnalysisResult.AnalysisResult);

            return new QueryAnalysisResultDto
            {
                Id = query.Id,
                DbConnectionId = query.DbConnectionId,
                Query = query.Sql,
                ExplainResult = query.ExplainResult,
                AlgorithmRecommendation = analysis.Recommendations,
                LlmRecommendations = analysis.LlmResult,
                FindindCustomRules = analysis.FindindCustomRules ?? [],
                ExplainComparisonDto = _explainer.Compare(query.ExplainResult, analysis.LlmResult?.ExplainResult)
            };
        }

        query = await _db.Queries.FirstOrDefaultAsync(x => x.Id == queryId);
        if (query == null)
        {
            throw new InvalidOperationException("Query not found");
        }

        var analysisResult = await _analyzer.GetAlgorithmResult(query.Sql, query.ExplainResult);
        var llmAnswer = useLlm ? await GetLlmResult(query) : null;
        var customFindings = await _customRulesService.ApplyForQuery(query, ruleIds ?? []);

        var result = new QueryAnalysisResult
        {
            QueryId = query.Id,
            Recommendations = analysisResult,
            LlmResult = llmAnswer,
            FindindCustomRules = customFindings.ToList()
        };

        _db.QueryAnalysisResults.Add(result);
        await _db.SaveChangesAsync();


        return new QueryAnalysisResultDto
        {
            Id = query.Id,
            DbConnectionId = query.DbConnectionId,
            Query = query.Sql,
            ExplainResult = query.ExplainResult,
            AlgorithmRecommendation = analysisResult,
            LlmRecommendations = llmAnswer,
            FindindCustomRules = customFindings,
            ExplainComparisonDto = _explainer.Compare(query.ExplainResult, llmAnswer?.ExplainResult)
        };
    }

    private async Task<SqlLlmAnalysisResult> GetLlmResult(QueryAnalysis query)
    {
        var llmAnswer = await _llm.GetRecommendation(
            query.Sql,
            query.ExplainResult
        );

        var dbConnection = await _db
            .DbConnections.FirstAsync(x => x.Id == query.DbConnectionId);

        var llmQueryExplain = await _explainer.Execute(dbConnection.GetConnectionString(), llmAnswer.NewQuery);
        return new SqlLlmAnalysisResult
        {
            LlmAnswer = llmAnswer,
            ExplainResult = llmQueryExplain
        };
    }

    private async Task UpdateQueryAnalysisResult(bool useLlm, IReadOnlyCollection<Guid>? ruleIds,
        QueryAnalysisResult analysisResult)
    {
        var isUpdated = false;
        if (useLlm && analysisResult.LlmResult == null)
        {
            var llmRecommendations = await GetLlmResult(analysisResult.Query);
            analysisResult.LlmResult = llmRecommendations;
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