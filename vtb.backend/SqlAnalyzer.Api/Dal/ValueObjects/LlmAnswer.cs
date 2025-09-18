using System.Text.Json.Serialization;
using SqlAnalyzer.Api.Services.LlmClient.Data;

namespace SqlAnalyzer.Api.Dal.ValueObjects;

public class LlmAnswer
{
    [JsonPropertyName("problems")] 
    public required IReadOnlyCollection<LlmAnswerPoint> Problems { get; init; }
    
    [JsonPropertyName("recommendations")]
    public required IReadOnlyCollection<LlmAnswerPoint> Recommendations { get; init; }
    
    [JsonPropertyName("newQuery")]
    public required string NewQuery { get; init; }
    
    [JsonPropertyName("newQueryAbout")]
    public required string NewQueryAbout {get; init; }
}