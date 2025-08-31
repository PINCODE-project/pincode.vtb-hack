using System.Text.Json.Serialization;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public sealed class LlmChatResponse
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("choices")] public List<ChatChoice> Choices { get; init; } = new();

}