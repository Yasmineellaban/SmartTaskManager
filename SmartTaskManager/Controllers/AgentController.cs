using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManager.Models;
using SmartTaskManager.Services.Agent;

namespace SmartTaskManager.Controllers;

[Authorize]
public class AgentController : Controller
{
    private readonly AgentService _agentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AgentController(
        AgentService agentService,
        UserManager<ApplicationUser> userManager)
    {
        _agentService = agentService;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> TestCategories()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result =
            await _agentService.GetCategoriesForAgentAsync(userId);

        return Content(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestFunctionCalling(string prompt)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _agentService.TestFunctionCallingAsync(
            prompt,
            userId);

        return Content(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestCreateTaskFunction(
    string prompt)
    {
        var result =
            await _agentService.TestCreateTaskFunctionAsync(prompt);

        return Content(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunAgent(string prompt)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _agentService.RunAgentAsync(
            prompt,
            userId);

        return Content(result);
    }
}