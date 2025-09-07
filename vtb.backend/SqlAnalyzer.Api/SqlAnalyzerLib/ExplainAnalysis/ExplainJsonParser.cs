using System.Globalization;
using System.Text.Json;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis;

/// <summary>
    /// Парсер EXPLAIN JSON; устойчиво обрабатывает стандартный формат PostgreSQL и извлекает основные поля.
    /// Внутри использует System.Text.Json и робастные методы извлечения значений (игнорируя регистр имён).
    /// </summary>
    public sealed class ExplainJsonParser : IExplainParser
    {
        /// <inheritdoc />
        public ExplainRootPlan Parse(string explainJson)
        {
            using var doc = JsonDocument.Parse(explainJson);
            var rootElement = doc.RootElement;

            JsonElement planElement;
            if (rootElement.ValueKind == JsonValueKind.Array)
            {
                if (rootElement.GetArrayLength() == 0) throw new ArgumentException("Explain JSON array is empty.");
                planElement = rootElement[0];
            }
            else
            {
                planElement = rootElement;
            }

            var planObj = FindPropertyIgnoreCase(planElement, "Plan") ?? throw new ArgumentException("Explain JSON does not contain 'Plan' node.");

            var rootNode = ParsePlanNode(planObj);

            var planningTime = TryGetDoubleIgnoreCase(planElement, "Planning Time");
            var executionTime = TryGetDoubleIgnoreCase(planElement, "Execution Time");

            var settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var settingsElement = FindPropertyIgnoreCase(planElement, "Settings");
            if (settingsElement.HasValue && settingsElement.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in settingsElement.Value.EnumerateObject())
                {
                    settings[p.Name] = p.Value.ToString() ?? "";
                }
            }

            var commandType = TryGetStringIgnoreCase(planElement, "Command Type") ?? TryGetStringIgnoreCase(planElement, "Command");

            return new ExplainRootPlan
            {
                CommandType = commandType,
                RootNode = rootNode,
                PlanningTimeMs = planningTime,
                ExecutionTimeMs = executionTime,
                Settings = settings.Count > 0 ? settings : null
            };
        }

        private static PlanNode ParsePlanNode(JsonElement elem)
        {
            var nodeType = TryGetStringIgnoreCase(elem, "Node Type") ?? TryGetStringIgnoreCase(elem, "NodeType") ?? "<unknown>";
            var startupCost = TryGetDoubleIgnoreCase(elem, "Startup Cost");
            var totalCost = TryGetDoubleIgnoreCase(elem, "Total Cost");
            var planRows = TryGetDoubleIgnoreCase(elem, "Plan Rows") ?? TryGetDoubleIgnoreCase(elem, "Plan Rows") ?? TryGetDoubleIgnoreCase(elem, "Plan Rows"); // defensive
            var planWidth = TryGetDoubleIgnoreCase(elem, "Plan Width");

            var actualStartup = TryGetDoubleIgnoreCase(elem, "Actual Startup Time");
            var actualTotal = TryGetDoubleIgnoreCase(elem, "Actual Total Time");
            var actualRows = TryGetDoubleIgnoreCase(elem, "Actual Rows");
            var actualLoops = TryGetIntIgnoreCase(elem, "Actual Loops");

            var nodeSpecific = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in elem.EnumerateObject())
            {
                var name = p.Name;
                if (IsStandardFieldName(name)) continue;
                object? value = null;
                switch (p.Value.ValueKind)
                {
                    case JsonValueKind.Number:
                        if (p.Value.TryGetInt64(out var i)) value = i;
                        else if (p.Value.TryGetDouble(out var d)) value = d;
                        else value = p.Value.GetRawText();
                        break;
                    case JsonValueKind.String:
                        value = p.Value.GetString();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        value = p.Value.GetBoolean();
                        break;
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        value = p.Value.GetRawText();
                        break;
                    case JsonValueKind.Null:
                        value = null;
                        break;
                }
                nodeSpecific[name] = value!;
            }

            var buffers = ParseBuffers(elem);

            List<PlanNode>? children = null;
            var childProps = new[] { "Plans", "Inner Plan", "Outer Plan", "Plan" };
            foreach (var cp in childProps)
            {
                var cElem = FindPropertyIgnoreCase(elem, cp);
                if (cElem.HasValue)
                {
                    var je = cElem.Value;
                    if (je.ValueKind == JsonValueKind.Array)
                    {
                        children ??= new List<PlanNode>();
                        foreach (var child in je.EnumerateArray())
                        {
                            children.Add(ParsePlanNode(child));
                        }
                    }
                    else if (je.ValueKind == JsonValueKind.Object)
                    {
                        children ??= new List<PlanNode>();
                        children.Add(ParsePlanNode(je));
                    }
                }
            }

            return new PlanNode
            {
                NodeType = nodeType,
                ShortNodeType = ShortNodeTypeFrom(nodeType),
                StartupCost = startupCost,
                TotalCost = totalCost,
                PlanRows = planRows,
                PlanWidth = planWidth,
                ActualStartupTimeMs = actualStartup,
                ActualTotalTimeMs = actualTotal,
                ActualRows = actualRows,
                ActualLoops = actualLoops,
                Buffers = buffers,
                NodeSpecific = nodeSpecific.Count > 0 ? nodeSpecific : null,
                Children = children
            };
        }

        private static BufferStats? ParseBuffers(JsonElement elem)
        {
            var be = FindPropertyIgnoreCase(elem, "Buffers");
            if (!be.HasValue) return null;
            var obj = be.Value;
            long sharedHit = TryGetLongIgnoreCase(obj, "Shared Hit") ?? TryGetLongIgnoreCase(obj, "shared hit") ?? 0;
            long sharedRead = TryGetLongIgnoreCase(obj, "Shared Read") ?? TryGetLongIgnoreCase(obj, "shared read") ?? 0;
            long localHit = TryGetLongIgnoreCase(obj, "Local Hit") ?? TryGetLongIgnoreCase(obj, "local hit") ?? 0;
            long localRead = TryGetLongIgnoreCase(obj, "Local Read") ?? TryGetLongIgnoreCase(obj, "local read") ?? 0;
            long tempRead = TryGetLongIgnoreCase(obj, "Temp Read") ?? TryGetLongIgnoreCase(obj, "temp read") ?? 0;
            long tempWritten = TryGetLongIgnoreCase(obj, "Temp Written") ?? TryGetLongIgnoreCase(obj, "temp written") ?? 0;

            return new BufferStats
            {
                SharedHit = sharedHit,
                SharedRead = sharedRead,
                LocalHit = localHit,
                LocalRead = localRead,
                TempRead = tempRead,
                TempWritten = tempWritten
            };
        }

        private static bool IsStandardFieldName(string name)
        {
            var lower = name.ToLowerInvariant();
            return lower == "node type" ||
                   lower == "nodetype" ||
                   lower == "startup cost" ||
                   lower == "total cost" ||
                   lower == "plan rows" ||
                   lower == "plan width" ||
                   lower == "actual startup time" ||
                   lower == "actual total time" ||
                   lower == "actual rows" ||
                   lower == "actual loops" ||
                   lower == "buffers" ||
                   lower == "plans" ||
                   lower == "join type" ||
                   lower == "index name";
        }

        private static string ShortNodeTypeFrom(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType)) return nodeType;
            if (nodeType.IndexOf("Seq Scan", StringComparison.OrdinalIgnoreCase) >= 0) return "SeqScan";
            if (nodeType.IndexOf("Index Scan", StringComparison.OrdinalIgnoreCase) >= 0) return "IndexScan";
            if (nodeType.IndexOf("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase) >= 0) return "BitmapHeapScan";
            if (nodeType.IndexOf("Hash Join", StringComparison.OrdinalIgnoreCase) >= 0) return "HashJoin";
            if (nodeType.IndexOf("Merge Join", StringComparison.OrdinalIgnoreCase) >= 0) return "MergeJoin";
            if (nodeType.IndexOf("Nested Loop", StringComparison.OrdinalIgnoreCase) >= 0) return "NestedLoop";
            if (nodeType.IndexOf("Sort", StringComparison.OrdinalIgnoreCase) >= 0) return "Sort";
            if (nodeType.IndexOf("Aggregate", StringComparison.OrdinalIgnoreCase) >= 0) return "Aggregate";
            if (nodeType.IndexOf("Hash", StringComparison.OrdinalIgnoreCase) >= 0) return "Hash";
            if (nodeType.IndexOf("Gather", StringComparison.OrdinalIgnoreCase) >= 0) return "Gather";
            return nodeType;
        }

        private static JsonElement? FindPropertyIgnoreCase(JsonElement elem, string propName)
        {
            if (elem.ValueKind != JsonValueKind.Object) return null;
            foreach (var p in elem.EnumerateObject())
            {
                if (string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase)) return p.Value;
            }
            return null;
        }

        private static string? TryGetStringIgnoreCase(JsonElement elem, string propName)
        {
            var p = FindPropertyIgnoreCase(elem, propName);
            if (!p.HasValue) return null;
            if (p.Value.ValueKind == JsonValueKind.String) return p.Value.GetString();
            return p.Value.ToString();
        }

        private static double? TryGetDoubleIgnoreCase(JsonElement elem, string propName)
        {
            var p = FindPropertyIgnoreCase(elem, propName);
            if (!p.HasValue) return null;
            if (p.Value.ValueKind == JsonValueKind.Number)
            {
                if (p.Value.TryGetDouble(out var d)) return d;
                if (p.Value.TryGetInt64(out var i)) return Convert.ToDouble(i);
            }
            var s = p.Value.ToString();
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            return null;
        }

        private static long? TryGetLongIgnoreCase(JsonElement elem, string propName)
        {
            var p = FindPropertyIgnoreCase(elem, propName);
            if (!p.HasValue) return null;
            if (p.Value.ValueKind == JsonValueKind.Number)
            {
                if (p.Value.TryGetInt64(out var i)) return i;
                if (p.Value.TryGetDouble(out var d)) return Convert.ToInt64(d);
            }
            var s = p.Value.ToString();
            if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            return null;
        }

        private static int? TryGetIntIgnoreCase(JsonElement elem, string propName)
        {
            var p = FindPropertyIgnoreCase(elem, propName);
            if (!p.HasValue) return null;
            if (p.Value.ValueKind == JsonValueKind.Number)
            {
                if (p.Value.TryGetInt32(out var i)) return i;
                if (p.Value.TryGetInt64(out var l)) return (int)l;
            }
            var s = p.Value.ToString();
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            return null;
        }
    }