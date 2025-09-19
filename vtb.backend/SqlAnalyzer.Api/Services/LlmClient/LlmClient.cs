using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SqlAnalyzer.Api.Dal.ValueObjects;
using SqlAnalyzer.Api.Services.LlmClient.Data;
using SqlAnalyzer.Api.Services.LlmClient.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzer.Api.Services.LlmClient;

using System.Text;
using System.Text.Json;

public sealed class LlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    private static readonly LlmMessage SystemMessage = new("system",
        "Ты эксперт по PostgreSQL и оптимизации SQL-запросов.\n" +
        "Я дам тебе SQL-запрос и результаты анализа его выполнения (EXPLAIN в формате JSON).\n" +
        "Отвечай только на русском языке! Выдавай все рекомендации и сообщения только на русском языке! Только кратко и по делу, без лишней воды!!!\n" +
        "Твоя задача:\n" +
        "1. Проанализировать исходный запрос и объяснить, какие есть проблемы в плане выполнения.\n" +
        "2. Предложить улучшенную версию запроса (сохранив логику, но оптимизировав joins, индексы, фильтры, агрегаты).\n" +
        "3. Объяснить, какие именно изменения ты внес и почему они ускорят выполнение.\n" +
        "4. Предупредить, если твои изменения могут повлиять на корректность результата.\n" +
        "5. Если запрос уже оптимален — объясни почему.\n\n" +
        "Формат ответа - JSON. В ответ отправляй только его и ничего больше:\n" +
        "- Анализ проблем в текущем плане (поле \"problems\" - массив сущностей {\"message\" : string, \"severity\" : enum (Info=0,Warning=1Critical=2)})\n" +
        "- Предложение по улучшению (поле \"recommendations\" - массив сущностей {\"message\" : string, \"severity\" : enum(Info=0,Warning=1Critical=2)})\n" +
        "- Новый улучшенный SQL-запрос (поле \"newQuery\" - строка)\n" +
        "- Обоснование изменений (поле \"newQueryAbout\" - строка)");

    public LlmClient(HttpClient httpClient)
    {
        _http = httpClient;
        _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (_http.Timeout == Timeout.InfiniteTimeSpan)
            _http.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<LlmAnswer> ChatAsync(
        IEnumerable<LlmMessage> messages,
        string model = "openai/gpt-oss-120b",
        double temperature = 1.0,
        bool stream = false,
        CancellationToken ct = default)
    {
        var payload = new LlmChatRequest
        {
            Model = model,
            Messages = messages.ToList(),
            Stream = stream,
            Temperature = temperature
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(payload, _json),
            Encoding.UTF8,
            "application/json");

        using var resp = await _http.PostAsync("/v1/chat/completions", content, ct);
        var body = JsonSerializer.Deserialize<JsonObject>(await resp.Content.ReadAsStringAsync(ct));
        var answerJson = (
                (JsonArray)
                (body.TryGetPropertyValue("choices", out var choice)
                    ? choice
                    : throw new InvalidOperationException("Empty LLM response.")
                ))
            .FirstOrDefault()
            ["message"]["content"].ToString();
        if (resp.IsSuccessStatusCode == false)
        {
            throw new HttpRequestException($"LLM HTTP {(int)resp.StatusCode}: {body}");
        }

        var parsed = JsonSerializer.Deserialize<LlmChatResponse>(body, _json)
                     ?? throw new InvalidOperationException("Empty LLM response.");

        var answer = JsonSerializer.Deserialize<LlmAnswer>(parsed.Choices.First().Message.Content, _json)
                     ?? throw new InvalidOperationException("Invalid LLM response.");
        return answer;
    }

    public Task<LlmAnswer> GetRecommendation(
        string originalSql,
        ExplainRootPlan? explainJson = null,
        string model = "openai/gpt-oss-120b",
        double temperature = 0.2,
        CancellationToken ct = default)
    {
        var userBuilder = new StringBuilder();
        userBuilder.AppendLine("Исходный SQL-запрос");
        userBuilder.AppendLine("```sql");
        userBuilder.AppendLine(originalSql);
        userBuilder.AppendLine("```");
        userBuilder.AppendLine();
        userBuilder.AppendLine("Результат вывода EXPLAIN (FORMAT JSON)::");
        userBuilder.AppendLine("```json");
        userBuilder.AppendLine(JsonSerializer.Serialize(explainJson));
        userBuilder.AppendLine("```");
        userBuilder.AppendLine();

        var user = new LlmMessage("user", userBuilder.ToString());

        return ChatAsync([SystemMessage, user], model, temperature, false, ct);
    }
}