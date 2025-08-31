using System.Text.Json.Serialization;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public class LlmAnswer
{
    [JsonPropertyName("problems")] 
    public required string Problems { get; init; }
    
    [JsonPropertyName("recommendations")]
    public required string Recommendations { get; init; }
    
    [JsonPropertyName("newQuery")]
    public required string NewQuery { get; init; }
    
    [JsonPropertyName("newQueryAbout")]
    public required string NewQueryAbout {get; init; }
}