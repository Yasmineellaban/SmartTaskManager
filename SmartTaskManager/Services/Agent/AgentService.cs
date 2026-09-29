using SmartTaskManager.Models;
using SmartTaskManager.Services;
using System.Text.Json;

namespace SmartTaskManager.Services.Agent;

public class AgentService
{
    private readonly GetCategoriesTool _getCategoriesTool;
    private readonly CreateTaskTool _createTaskTool;
    private readonly GeminiService _geminiService;

    public AgentService(
        GetCategoriesTool getCategoriesTool,
        CreateTaskTool createTaskTool,
        GeminiService geminiService)
    {
        _getCategoriesTool = getCategoriesTool;
        _createTaskTool = createTaskTool;
        _geminiService = geminiService;
    }

    public async Task<string> GetCategoriesForAgentAsync(
        string userId)
    {
        var categories =
            await _getCategoriesTool.GetCategoriesAsync(userId);

        if (categories.Count == 0)
        {
            return "The user has no categories.";
        }

        return string.Join(
            "\n",
            categories.Select(c =>
                $"Id: {c.Id}, Name: {c.Name}"));
    }

    public async Task<string> TestFunctionCallingAsync(
        string prompt,
        string userId)
    {
        var responseJson =
            await _geminiService.AskWithCategoryToolAsync(prompt);

        using var document =
            JsonDocument.Parse(responseJson);

        var root =
            document.RootElement;

        var candidate =
            root.GetProperty("candidates")[0];

        var content =
            candidate.GetProperty("content");

        var parts =
            content.GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty(
                    "functionCall",
                    out var functionCall))
            {
                var functionName =
                    functionCall
                        .GetProperty("name")
                        .GetString();

                if (functionName == "get_categories")
                {
                    return await GetCategoriesForAgentAsync(
                        userId);
                }
            }
        }

        return "Gemini did not request the get_categories tool.";
    }

    public async Task<string> TestCreateTaskFunctionAsync(
        string prompt)
    {
        var agentPrompt =
            "You are a task management agent. " +
            "When the user asks you to create a task, " +
            "you MUST use the create_task function. " +
            "The create_task function requires a title, " +
            "priority, and categoryId. " +
            "User request: " +
            prompt;

        var responseJson =
            await _geminiService.AskWithCategoryToolAsync(
                agentPrompt);

        using var document =
            JsonDocument.Parse(responseJson);

        var root =
            document.RootElement;

        var candidate =
            root.GetProperty("candidates")[0];

        var content =
            candidate.GetProperty("content");

        var parts =
            content.GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty(
                    "functionCall",
                    out var functionCall))
            {
                var functionName =
                    functionCall
                        .GetProperty("name")
                        .GetString();

                if (functionName == "create_task")
                {
                    var args =
                        functionCall.GetProperty("args");

                    return JsonSerializer.Serialize(
                        new
                        {
                            Function = functionName,
                            Arguments = args
                        },
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                }
            }
        }

        return "Gemini did not request the create_task tool.";
    }

    public async Task<string> RunAgentAsync(
        string prompt,
        string userId)
    {
        var agentPrompt =
            "You are a task management agent. " +
            "You have access to two tools: get_categories and create_task. " +
            "If the user asks to create a task and mentions a category name, " +
            "you MUST call get_categories first to find the correct categoryId. " +
            "Never guess a categoryId. " +
            "Only call create_task after you know the correct categoryId. " +
            "If the user specifies a due date or time such as tomorrow, today, " +
            "next week, next Monday, or a specific date, " +
            "you MUST provide the dueDate argument in the create_task function call. " +
            "Calculate relative dates based on today's date. " +
            "If the user does not specify a due date, leave dueDate empty. " +
            "Use ISO 8601 format for dueDate. " +
            "Use the exact categoryId returned by get_categories. " +
            "User request: " +
             prompt;


        var responseJson =
            await _geminiService.AskWithCategoryToolAsync(
                agentPrompt);

        for (int i = 0; i < 5; i++)
        {
            using var document =
                JsonDocument.Parse(responseJson);

            var root =
                document.RootElement;

            var candidate =
                root.GetProperty("candidates")[0];

            var content =
                candidate.GetProperty("content");

            var parts =
                content.GetProperty("parts");

            JsonElement functionCall =
                default;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty(
                        "functionCall",
                        out var call))
                {
                    functionCall = call;
                    break;
                }
            }

            if (functionCall.ValueKind ==
                JsonValueKind.Undefined)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty(
                            "text",
                            out var text))
                    {
                        return text.GetString()
                            ?? string.Empty;
                    }
                }

                return "The agent returned no result.";
            }

            var functionName =
                functionCall
                    .GetProperty("name")
                    .GetString();

            var functionArgs =
                functionCall.TryGetProperty(
                    "args",
                    out var args)
                    ? args
                    : new JsonElement();

            string functionResult;

            if (functionName == "get_categories")
            {
                functionResult =
                    await GetCategoriesForAgentAsync(
                        userId);
            }
            else if (functionName == "create_task")
            {
                if (!functionArgs.TryGetProperty(
                        "title",
                        out var titleElement))
                {
                    return "The agent did not provide a task title.";
                }

                if (!functionArgs.TryGetProperty(
                        "priority",
                        out var priorityElement))
                {
                    return "The agent did not provide a task priority.";
                }

                if (!functionArgs.TryGetProperty(
                        "categoryId",
                        out var categoryIdElement))
                {
                    return "The agent did not provide a category.";
                }

                var title =
                    titleElement.GetString();

                var description =
                    functionArgs.TryGetProperty(
                        "description",
                        out var descriptionElement)
                        ? descriptionElement.GetString()
                        : null;

                var priorityText =
                    priorityElement.GetString();

                var categoryId =
                    categoryIdElement.GetInt32();

                DateTime? dueDate = null;

                if (functionArgs.TryGetProperty(
                        "dueDate",
                        out var dueDateElement))
                {
                    var dueDateText =
                        dueDateElement.GetString();

                    if (!string.IsNullOrWhiteSpace(dueDateText) &&
                        DateTime.TryParse(
                            dueDateText,
                            out var parsedDate))
                    {
                        dueDate = parsedDate;
                    }
                }

                if (prompt.Contains(
                        "tomorrow",
                        StringComparison.OrdinalIgnoreCase))
                {
                    dueDate =
                        DateTime.Today.AddDays(1);
                }

                if (!Enum.TryParse<TaskPriority>(
                        priorityText,
                        true,
                        out var priority))
                {
                    return
                        $"Invalid priority: {priorityText}";
                }

                var task =
                    await _createTaskTool.CreateTaskAsync(
                        title!,
                        description,
                        priority,
                        dueDate,
                        categoryId,
                        userId);

                functionResult =
                    $"Task created successfully. " +
                    $"Title: {task.Title}, " +
                    $"Priority: {task.Priority}, " +
                    $"Category ID: {task.CategoryId}, " +
                    $"Due Date: {task.DueDate}";
            }
            else
            {
                return
                    $"Unknown function: {functionName}";
            }

            responseJson =
                await _geminiService.SendFunctionResultAsync(
                    agentPrompt,
                    content,
                    functionName!,
                    functionResult);
        }

        return "The agent could not complete the request.";
    }
}