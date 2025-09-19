using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Algorithm;

namespace SqlAnalyzer.Api.Services.RuleSeeder;

public static class CustomRuleSeeder
{
    private const string Path = "baseSqlRules.json";
        
    public static async Task AddCustomRules(DataContext db)
    {
        if (await db.SqlAnalyzeRules.CountAsync() > 0)
        {
            return;
        }

        var rules = JsonSerializer.Deserialize<IReadOnlyCollection<SqlAnalyzeRule>>(
            await File.ReadAllTextAsync(Path),
            new JsonSerializerOptions {PropertyNameCaseInsensitive = true}
            
            );
        if (rules is not null && rules.Count > 0)
        {
            db.SqlAnalyzeRules.AddRange(rules);
            await db.SaveChangesAsync();
        }
    }
}