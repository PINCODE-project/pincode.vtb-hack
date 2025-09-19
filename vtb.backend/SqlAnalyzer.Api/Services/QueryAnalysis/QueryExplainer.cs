using Npgsql;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Services.QueryAnalysis;

public class QueryExplainer : IQueryExplainer
{
    private readonly ILogger<IQueryExplainer> _logger;
    private readonly IExplainParser _explainParser;

    public QueryExplainer(ILogger<IQueryExplainer> logger, IExplainParser explainParser)
    {
        _logger = logger;
        _explainParser = explainParser;
    }

    public async Task<ExplainRootPlan?> Execute(string dbConnectionString, string query)
    {
        ExplainRootPlan? explainRootPlan = null;
        try
        {
            await using var conn = new NpgsqlConnection(dbConnectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand($"EXPLAIN (FORMAT JSON) {query}", conn);
            var result = await cmd.ExecuteScalarAsync();

            var analyzeResult = result switch
            {
                string str => str,
                string[] arr => string.Join(Environment.NewLine, arr),
                _ => throw new Exception("Unexpected EXPLAIN output")
            };

            explainRootPlan = _explainParser.Parse(analyzeResult);
        }
        catch
        {
            _logger.LogError("Failed to execute explain");
        }

        return explainRootPlan;
    }
    
    public PlanComparisonDto? Compare(ExplainRootPlan? oldPlan, ExplainRootPlan? newPlan)
    {
        if (oldPlan is null || newPlan is null)
        {
            return null;
        }
        
        var oldStats = AggregatePlan(oldPlan);
        var newStats = AggregatePlan(newPlan);

        return new PlanComparisonDto
        {
            Cost = new PlanPointComparsionResult(
                oldStats.TotalCost, newStats.TotalCost, CalcPercent(oldStats.TotalCost, newStats.TotalCost)
                ),
            Rows = new PlanPointComparsionResult(
                oldStats.EstimatedRows, newStats.EstimatedRows, CalcPercent(oldStats.EstimatedRows, newStats.EstimatedRows)
                ),
            Width = new PlanPointComparsionResult(
                oldStats.AvgWidth, newStats.AvgWidth, CalcPercent(oldStats.AvgWidth, newStats.AvgWidth)),

            SeqScanCount = new PlanPointComparsionResult(
                oldStats.SeqScanCount, newStats.SeqScanCount, CalcPercent(oldStats.SeqScanCount, newStats.SeqScanCount)),
            NodeCount = new PlanPointComparsionResult(
                oldStats.NodeCount, newStats.NodeCount, CalcPercent(oldStats.NodeCount, newStats.NodeCount)),

            OldJoinTypes = string.Join(", ", oldStats.JoinTypes),
            NewJoinTypes = string.Join(", ", newStats.JoinTypes)
        };
    }

    private static PlanAggregate AggregatePlan(ExplainRootPlan root)
    {
        var nodes = Flatten(root.RootNode);

        return new PlanAggregate
        {
            TotalCost = (decimal)root.RootNode.TotalCost!,
            EstimatedRows = (long)nodes.Sum(n => n.PlanRows)!,
            AvgWidth = nodes.Any() ? (int)nodes.Average(n => n.PlanWidth)! : 0,
            SeqScanCount = nodes.Count(n => n.NodeType == "Seq Scan"),
            NodeCount = nodes.Count,
            JoinTypes = nodes
                .Where(n => n.NodeType.Contains("Join"))
                .Select(n => n.NodeType)
                .Distinct()
                .ToList()
        };
    }

    private static List<PlanNode> Flatten(PlanNode node)
    {
        var list = new List<PlanNode> { node };
        if (node.Children != null)
        {
            foreach (var child in node.Children)
                list.AddRange(Flatten(child));
        }
        return list;
    }

    private static decimal? CalcPercent(decimal oldValue, decimal newValue)
    {
        if (oldValue == 0) return null;
        return (oldValue - newValue) / oldValue * 100;
    }

    private class PlanAggregate
    {
        public decimal TotalCost { get; set; }
        public long EstimatedRows { get; set; }
        public int AvgWidth { get; set; }
        public int SeqScanCount { get; set; }
        public int NodeCount { get; set; }
        public List<string> JoinTypes { get; set; } = new();
    }
}