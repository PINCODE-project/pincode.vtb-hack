using System.Text.Json.Serialization;
using SqlAnalyzer.Api.Services.LlmClient.Data;
using SqlAnalyzer.Api.Services.LlmClient.Interfaces;

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
        "Отвечай только на русском языке.\n" +
        "Твоя задача:\n" +
        "1. Проанализировать исходный запрос и объяснить, какие есть проблемы в плане выполнения.\n" +
        "2. Предложить улучшенную версию запроса (сохранив логику, но оптимизировав joins, индексы, фильтры, агрегаты).\n" +
        "3. Объяснить, какие именно изменения ты внес и почему они ускорят выполнение.\n" +
        "4. Предупредить, если твои изменения могут повлиять на корректность результата.\n" +
        "5. Если запрос уже оптимален — объясни почему.\n\n" +
        "Формат ответа - JSON. В ответ отправляй только его и ничего больше:\n" +
        "- Анализ проблем в текущем плане (поле \"problems\")\n" +
        "- Предложение по улучшению (поле \"recommendations\")\n" +
        "- Новый улучшенный SQL-запрос (поле \"newQuery\")\n" +
        "- Обоснование изменений (поле \"newQueryAbout\")");

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
        var body = await resp.Content.ReadAsStringAsync(ct);

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

    public Task<LlmAnswer> GetRecommendationAsync(
        string findings,
        string originalSql,
        string? explainJson = null,
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
        userBuilder.AppendLine(explainJson);
        userBuilder.AppendLine("```");
        userBuilder.AppendLine();
        userBuilder.AppendLine("Выявленные проблемы:");
        userBuilder.AppendLine(findings);
        userBuilder.AppendLine();

        var user = new LlmMessage("user", userBuilder.ToString());

        return ChatAsync([SystemMessage, user], model, temperature, false, ct);
    }
}
