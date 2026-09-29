using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.Models;

namespace SmartTaskManager.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var tasks = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .ToListAsync();

        ViewBag.TotalTasks = tasks.Count;

        ViewBag.PendingTasks = tasks.Count(t =>
            t.Status == SmartTaskManager.Models.TaskStatus.Pending);

        ViewBag.InProgressTasks = tasks.Count(t =>
            t.Status == SmartTaskManager.Models.TaskStatus.InProgress);

        ViewBag.CompletedTasks = tasks.Count(t =>
            t.Status == SmartTaskManager.Models.TaskStatus.Completed);

        ViewBag.OverdueTasks = tasks.Count(t =>
            t.DueDate.HasValue &&
            t.DueDate.Value < DateTime.Now &&
            t.Status != SmartTaskManager.Models.TaskStatus.Completed &&
            t.Status != SmartTaskManager.Models.TaskStatus.Cancelled);

        ViewBag.CompletionRate = tasks.Count == 0
        ? 0
        : Math.Round(
            (double)tasks.Count(t =>
                t.Status == SmartTaskManager.Models.TaskStatus.Completed)
                / tasks.Count * 100,
                0);

        ViewBag.RecentTasks = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Include(t => t.Category)
            .ToListAsync();

        return View();
    }
}