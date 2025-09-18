using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Entensions;

public static class PlanNodeExtensions
{
    private const string RelationName = "RelationName";
    
    public static bool ContainsInNodeSpecific(this PlanNode node, string key)
    {
        return node.NodeSpecific != null && node.NodeSpecific.ContainsKey(key);
    }
    
    public static string? TryGetNodeSpecificString(this PlanNode node, string key)
    {
        return node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null
            ? v.ToString()
            : null;
    }
    
    public static string GetRelationName(this PlanNode node) => TryGetNodeSpecificString(node, RelationName) ?? "unknown";
}