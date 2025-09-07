using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.LlmClient.Interfaces;
using SqlAnalyzer.Api.Services.Recomedation.Interfaces;

namespace SqlAnalyzer.Api.Services.Recomedation;

public class QueryRecommendationService : IQueryRecommendationService
{
    private readonly DataContext _db;
    private readonly ILlmClient _llm;

    public QueryRecommendationService(DataContext db, ILlmClient llm)
    {
        _db = db;
        _llm = llm;
    }

    public async Task<QueryAnalysisResultDto> GetRecommendations(Guid queryAnalysisId)
    {
        var query = await _db.QueryAnalysis.FirstAsync(x => x.Id == queryAnalysisId);
        var algorithmRecommendation = AnalyzeExplainJson(query.AnalyzeResult);
        var llmRecommendation =
            await _llm.GetRecommendationAsync(algorithmRecommendation, query.Query, query.AnalyzeResult);
        return new QueryAnalysisResultDto(
            query.Id,
            query.DbConnectionId,
            query.Query,
            query.AnalyzeResult,
            algorithmRecommendation,
            llmRecommendation
        );
    }

    private string AnalyzeExplainJson(string explainJson)
    {
        using var doc = JsonDocument.Parse(explainJson);
        var root = doc.RootElement[0].GetProperty("Plan");

        var findings = new List<string>();

        void Walk(JsonElement plan, int depth = 0)
        {
            var nodeType = plan.GetProperty("Node Type").GetString() ?? "Unknown";

            var estRows = plan.TryGetProperty("Plan Rows", out var pr) ? pr.GetInt64() : -1;
            var actRows = plan.TryGetProperty("Actual Rows", out var ar) ? ar.GetInt64() : -1;

            var totalCost = plan.TryGetProperty("Total Cost", out var tc) ? tc.GetDouble() : 0;
            var startupCost = plan.TryGetProperty("Startup Cost", out var sc) ? sc.GetDouble() : 0;

            var rel = plan.TryGetProperty("Relation Name", out var relName) ? relName.GetString() : null;
            var filter = plan.TryGetProperty("Filter", out var f) ? f.GetString() : null;
            var indexCond = plan.TryGetProperty("Index Cond", out var ic) ? ic.GetString() : null;

            // ---- RULES ----

            // Seq Scan без индекса на большой таблице
            if (nodeType == "Seq Scan" && estRows > 100_000 && string.IsNullOrEmpty(indexCond))
            {
                findings.Add($"❌ Seq Scan on {rel} (~{estRows} rows). Consider adding an index.");
            }

            // Seq Scan на маленькой таблице — ок
            if (nodeType == "Seq Scan" && estRows <= 100_000)
            {
                findings.Add($"⚠️ Seq Scan on {rel} (~{estRows} rows). Might be acceptable if table is small.");
            }

            // Index Scan / Bitmap Index Scan — good
            if (nodeType.Contains("Index Scan"))
            {
                findings.Add($"✅ {nodeType} on {rel}, condition: {indexCond}");
            }

            // Nested Loop с большими таблицами
            if (nodeType == "Nested Loop" && estRows > 100_000)
            {
                findings.Add($"❌ Nested Loop with {estRows} estimated rows. Consider Hash Join / Merge Join.");
            }

            // Hash Join
            if (nodeType == "Hash Join")
            {
                findings.Add($"ℹ️ Hash Join detected (~{estRows} rows). Ensure join keys are indexed.");
            }

            // Merge Join
            if (nodeType == "Merge Join")
            {
                findings.Add($"ℹ️ Merge Join (~{estRows} rows). Ensure input is sorted or indexed.");
            }

            // Проверка переоценки/недооценки строк
            if (estRows > 0 && actRows > 0)
            {
                double ratio = (double)actRows / estRows;
                if (ratio > 5)
                    findings.Add($"❌ Cardinality mismatch: estimated {estRows}, actual {actRows} (underestimation).");
                else if (ratio < 0.2)
                    findings.Add($"❌ Cardinality mismatch: estimated {estRows}, actual {actRows} (overestimation).");
            }

            // Сортировка на большом объёме
            if (nodeType.Contains("Sort") && estRows > 50_000)
            {
                findings.Add($"⚠️ Sort on {estRows} rows. Consider adding index to avoid explicit sort.");
            }

            // Агрегация на большом объёме
            if (nodeType.Contains("Aggregate") && estRows > 100_000)
            {
                findings.Add($"⚠️ Aggregate on {estRows} rows. Consider pre-aggregating or indexing.");
            }

            // Параллелизм
            if (plan.TryGetProperty("Parallel Aware", out var par) && par.GetBoolean())
            {
                findings.Add($"ℹ️ Parallel plan used for {nodeType} (~{estRows} rows). Good for performance.");
            }

            // Рекурсивный обход
            if (plan.TryGetProperty("Plans", out var subPlans))
            {
                foreach (var sub in subPlans.EnumerateArray())
                    Walk(sub, depth + 1);
            }
        }

        Walk(root);

        return findings.Count > 0
            ? string.Join("\n", findings)
            : "✅ No critical issues found in the plan.";
    }
}