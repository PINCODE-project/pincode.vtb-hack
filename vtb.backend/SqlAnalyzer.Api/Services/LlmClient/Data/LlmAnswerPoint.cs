using System.Text.Json.Serialization;
using SqlAnalyzer.Api.Dal.Constants;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public class LlmAnswerPoint
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    [JsonPropertyName("severity")]
    public required Severity Severity { get; init; }
}