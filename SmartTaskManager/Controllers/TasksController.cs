using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.Models;

namespace SmartTaskManager.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(
        string? search,
        SmartTaskManager.Models.TaskStatus? status,
        SmartTaskManager.Models.TaskPriority? priority,
        int? categoryId)
    {
        var userId = _userManager.GetUserId(User);

        var query = _context.TaskItems
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Title.Contains(search) ||
                (t.Description != null && t.Description.Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        var tasks = await query.ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Priority = priority;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categories;

        return View(tasks);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = _userManager.GetUserId(User);

        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        var userId = _userManager.GetUserId(User);

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == task.CategoryId && c.UserId == userId);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                "CategoryId",
                "Please select a valid category.");
        }

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(task);
        }

        task.UserId = userId!;

        _context.TaskItems.Add(task);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var task = await _context.TaskItems
            .Include(t => t.Category)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories;

        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskItem task)
    {
        if (id != task.Id)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var existingTask = await _context.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (existingTask == null)
        {
            return NotFound();
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c =>
                c.Id == task.CategoryId &&
                c.UserId == userId);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                "CategoryId",
                "Please select a valid category.");
        }

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(task);
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Priority = task.Priority;
        existingTask.Status = task.Status;
        existingTask.DueDate = task.DueDate;
        existingTask.CategoryId = task.CategoryId;

        if (task.Status == SmartTaskManager.Models.TaskStatus.Completed)
        {
            if (existingTask.CompletedAt == null)
            {
                existingTask.CompletedAt = DateTime.UtcNow;
            }
        }
        else
        {
            existingTask.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Details),
            new { id = existingTask.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var task = await _context.TaskItems
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        _context.TaskItems.Remove(task);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}