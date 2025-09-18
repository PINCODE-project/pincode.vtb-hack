using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Base;
using SqlAnalyzer.Api.Dal.Entities.Algorithm;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.SqlAnalyzeRule;
using SqlAnalyzer.Api.Services.Algorithm.Interfaces;

namespace SqlAnalyzer.Api.Services.Algorithm;

public class SqlAnalyzeRuleService : ISqlAnalyzeRuleService
{
    private readonly DataContext _db;

    public SqlAnalyzeRuleService(DataContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<SqlAnalyzeRuleDto>> Find(int? skip, int? take)
    {
        var rules = await _db
            .SqlAnalyzeRules
            .AsNoTracking()
            .UseLimiter(skip, take)
            .Select(rule => new SqlAnalyzeRuleDto
                {
                    Id = rule.Id,
                    Name = rule.Name,
                    Severity = rule.Severity,
                    Problem = rule.Problem,
                    Recommendation = rule.Recommendation,
                    Regex = rule.Regex,
                    CreatedAt = rule.CreateAt,
                    IsActive = rule.IsActive,
                }
            )
            .ToListAsync();

        return rules;
    }

    public async Task<Guid> Create(SqlAnalyzeRuleCreateDto dto)
    {
        var rule = new SqlAnalyzeRule
        {
            Name = dto.Name,
            Severity = dto.Severity,
            Problem = dto.Problem,
            Recommendation = dto.Recommendation,
            Regex = dto.Regex,
            IsActive = true,
        };
        
        _db.SqlAnalyzeRules.Add(rule);
        await _db.SaveChangesAsync();

        return rule.Id;
    }

    public async Task Update(SqlAnalyzeRuleUpdateDto dto)
    {
        var rule = await _db.SqlAnalyzeRules.FirstAsync(r => r.Id == dto.Id);

        if (string.IsNullOrEmpty(dto.Name) == false)
        {
            rule.Name = dto.Name;
        }
        
        if (string.IsNullOrEmpty(dto.Problem) == false)
        {
            rule.Problem = dto.Problem;
        }
        
        if (string.IsNullOrEmpty(dto.Recommendation) == false)
        {
            rule.Recommendation = dto.Recommendation;
        }
        
        if (string.IsNullOrEmpty(dto.Regex) == false)
        {
            rule.Regex = dto.Regex;
        }

        if (dto.Severity is not null)
        {
            rule.Severity = dto.Severity.Value;
        }

        if (dto.IsActive is not null)
        {
            rule.IsActive = dto.IsActive.Value;
        }

        await _db.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        await _db.SqlAnalyzeRules.Where(r => r.Id == id).ExecuteDeleteAsync();
    }

    public async Task<IReadOnlyCollection<Guid>> ApplyForQuery(Guid queryId, params IReadOnlyCollection<Guid> ruleIds)
    {
        var query = await _db.Queries.FirstAsync(x => x.Id == queryId);

        var rulesQuery = _db.SqlAnalyzeRules.AsNoTracking().Where(x => x.IsActive);
        if (ruleIds.Count > 0)
        {
            rulesQuery = rulesQuery.Where(x => ruleIds.Contains(x.Id));
        }
        
        var rules = await rulesQuery.ToListAsync();

        var result = new List<Guid>();
        foreach (var rule in rules)
        {
            if (Regex.IsMatch(query.Sql, rule.Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled))
            {
                result.Add(rule.Id);
            }
        }

        return result;
    }
}