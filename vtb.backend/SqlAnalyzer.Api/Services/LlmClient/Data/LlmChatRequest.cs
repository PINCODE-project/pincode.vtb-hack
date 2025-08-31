using System.Text.Json.Serialization;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public sealed class LlmChatRequest
{
    [JsonPropertyName("model")] 
    public required string Model { get; init; }
    
    [JsonPropertyName("messages")] 
    public required List<LlmMessage> Messages { get; init; } = [];
    
    [JsonPropertyName("stream")]
    public required bool Stream { get; init; } = false;
    
    [JsonPropertyName("temperature")]
    public required double Temperature { get; init; }
}