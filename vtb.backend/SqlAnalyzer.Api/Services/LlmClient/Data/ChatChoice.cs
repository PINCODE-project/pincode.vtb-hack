using System.Text.Json.Serialization;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public sealed class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; init; }
    
    [JsonPropertyName("message")]
    public LlmMessage Message { get; init; } = new("assistant", "");
    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}