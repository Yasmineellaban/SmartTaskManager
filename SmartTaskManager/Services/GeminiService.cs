using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace SmartTaskManager.Services;

public class GeminiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public GeminiService(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _apiKey = _configuration["Gemini:ApiKey"];
    }

    public async Task<string> AskAsync(string prompt)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody);

        using var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.PostAsync(
                url,
                content);

        response.EnsureSuccessStatusCode();

        var responseJson =
            await response.Content.ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(responseJson);

        var answer =
            document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

        return answer ?? string.Empty;
    }

    public async IAsyncEnumerable<string> AskStreamAsync(
        string prompt,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:streamGenerateContent?alt=sse&key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                url);

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var reader =
            new StreamReader(stream);

        while (true)
        {
            var line =
                await reader.ReadLineAsync(
                    cancellationToken);

            if (line == null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!line.StartsWith("data:"))
            {
                continue;
            }

            var data =
                line["data:".Length..].Trim();

            if (data == "[DONE]")
            {
                yield break;
            }

            using var document =
                JsonDocument.Parse(data);

            if (!document.RootElement.TryGetProperty(
                    "candidates",
                    out var candidates))
            {
                continue;
            }

            if (candidates.GetArrayLength() == 0)
            {
                continue;
            }

            var candidate =
                candidates[0];

            if (!candidate.TryGetProperty(
                    "content",
                    out var contentElement))
            {
                continue;
            }

            if (!contentElement.TryGetProperty(
                    "parts",
                    out var parts))
            {
                continue;
            }

            if (parts.GetArrayLength() == 0)
            {
                continue;
            }

            if (!parts[0].TryGetProperty(
                    "text",
                    out var textElement))
            {
                continue;
            }

            var text =
                textElement.GetString();

            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }
    }

    public async Task<string> AskWithCategoryToolAsync(
        string prompt)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },

            tools = GetAgentTools(),

            toolConfig = new
            {
                functionCallingConfig = new
                {
                    mode = "AUTO"
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody);

        using var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.PostAsync(
                url,
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Gemini API error: {response.StatusCode}\n{error}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> SendFunctionResultAsync(
        string originalPrompt,
        JsonElement modelContent,
        string functionName,
        string functionResult,
        string? functionCallId = null)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={_apiKey}";

        var functionResponse =
            new Dictionary<string, object?>
            {
                ["name"] = functionName,

                ["response"] = new
                {
                    result = functionResult
                }
            };

        if (!string.IsNullOrWhiteSpace(functionCallId))
        {
            functionResponse["id"] =
                functionCallId;
        }

        var requestBody = new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",

                    parts = new object[]
                    {
                        new
                        {
                            text = originalPrompt
                        }
                    }
                },

                modelContent,

                new
                {
                    role = "user",

                    parts = new object[]
                    {
                        new
                        {
                            functionResponse
                        }
                    }
                }
            },

            tools = GetAgentTools(),

            toolConfig = new
            {
                functionCallingConfig = new
                {
                    mode = "AUTO"
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody);

        using var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.PostAsync(
                url,
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Gemini API error: {response.StatusCode}\n{error}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    private object[] GetAgentTools()
    {
        return new object[]
        {
            new
            {
                functionDeclarations = new object[]
                {
                    new
                    {
                        name = "get_categories",

                        description =
                            "Gets all categories belonging to the current user.",

                        parameters = new
                        {
                            type = "object",

                            properties = new { },

                            required =
                                Array.Empty<string>()
                        }
                    },

                    new
                    {
                        name = "create_task",

                        description =
                            "Creates a new task for the current user.",

                        parameters = new
                        {
                            type = "object",

                            properties = new
                            {
                                title = new
                                {
                                    type = "string",

                                    description =
                                        "The title of the task."
                                },

                                description = new
                                {
                                    type = "string",

                                    description =
                                        "A detailed description of the task."
                                },

                                priority = new
                                {
                                    type = "string",

                                    description =
                                        "The priority of the task: Low, Medium, High, or Urgent."
                                },

                                dueDate = new
                                {
                                    type = "string",

                                    description =
                                        "The task due date in ISO 8601 format, if the user specifies one."
                                },

                                categoryId = new
                                {
                                    type = "integer",

                                    description =
                                        "The exact category ID returned by get_categories."
                                }
                            },

                            required = new[]
                            {
                                "title",
                                "priority",
                                "categoryId"
                            }
                        }
                    }
                }
            }
        };
    }
}