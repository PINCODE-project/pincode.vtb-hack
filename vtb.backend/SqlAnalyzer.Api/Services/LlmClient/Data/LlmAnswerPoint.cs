using System.Text.Json.Serialization;
using SqlAnalyzerLib.Common;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public class LlmAnswerPoint
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    [JsonPropertyName("severity")]
    public required AnalysisSeverity Severity { get; init; }
}