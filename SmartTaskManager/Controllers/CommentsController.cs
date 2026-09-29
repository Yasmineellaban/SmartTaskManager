using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.Models;

namespace SmartTaskManager.Controllers;

[Authorize]
public class CommentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int taskItemId,
        string content)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == taskItemId &&
                t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["CommentError"] = "Comment cannot be empty.";

            return RedirectToAction(
                "Details",
                "Tasks",
                new { id = taskItemId });
        }

        var comment = new TaskComment
        {
            Content = content,
            TaskItemId = taskItemId,
            UserId = userId!
        };

        _context.TaskComments.Add(comment);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Tasks",
            new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);

        var comment = await _context.TaskComments
            .Include(c => c.TaskItem)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.UserId == userId);

        if (comment == null)
        {
            return NotFound();
        }

        var taskId = comment.TaskItemId;

        _context.TaskComments.Remove(comment);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Tasks",
            new { id = taskId });
    }
}