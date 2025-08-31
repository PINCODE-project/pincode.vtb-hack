using System.Text.Json.Serialization;

namespace SqlAnalyzer.Api.Services.LlmClient.Data;

public sealed record LlmMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")]
    string Content
);