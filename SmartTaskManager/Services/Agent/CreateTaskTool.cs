using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.Models;

namespace SmartTaskManager.Services.Agent;

public class CreateTaskTool
{
    private readonly ApplicationDbContext _context;

    public CreateTaskTool(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem> CreateTaskAsync(
        string title,
        string? description,
        TaskPriority priority,
        DateTime? dueDate,
        int categoryId,
        string userId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == categoryId &&
                c.UserId == userId);

        if (category == null)
        {
            throw new InvalidOperationException(
                "Category not found.");
        }

        var task = new TaskItem
        {
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            CategoryId = categoryId,
            UserId = userId,
            Status = SmartTaskManager.Models.TaskStatus.Pending
        };

        _context.TaskItems.Add(task);

        await _context.SaveChangesAsync();

        return task;
    }
}