using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManager.Services;

namespace SmartTaskManager.Controllers;

[Authorize]
public class AIController : Controller
{
    private readonly GeminiService _geminiService;

    public AIController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return RedirectToAction(nameof(Index));
        }

        var answer = await _geminiService.AskAsync(prompt);

        ViewBag.Answer = Markdown.ToHtml(answer);

        return View("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task Stream(string prompt)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append(
            "X-Accel-Buffering",
            "no");

        await foreach (var chunk in
            _geminiService.AskStreamAsync(prompt))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    text = chunk
                });

            await Response.WriteAsync($"data: {json}\n\n");

            await Response.Body.FlushAsync();
        }
    }
}