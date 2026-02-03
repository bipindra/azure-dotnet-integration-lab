using DbAzureSqlEFCore.Data;
using DbAzureSqlEFCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbAzureSqlEFCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(ApplicationDbContext context, ILogger<TodoItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all todo items.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        _logger.LogInformation("Getting all todo items");
        var items = await _context.TodoItems.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Get a todo item by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
    {
        _logger.LogInformation("Getting todo item {Id}", id);
        var item = await _context.TodoItems.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Create a new todo item.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem item)
    {
        _logger.LogInformation("Creating todo item: {Title}", item.Title);
        
        item.CreatedAt = DateTime.UtcNow;
        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, item);
    }

    /// <summary>
    /// Update a todo item.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoItem(int id, TodoItem item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        _logger.LogInformation("Updating todo item {Id}", id);

        var existingItem = await _context.TodoItems.FindAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Title = item.Title;
        existingItem.Description = item.Description;
        existingItem.IsCompleted = item.IsCompleted;
        existingItem.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TodoItemExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a todo item.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        _logger.LogInformation("Deleting todo item {Id}", id);
        var item = await _context.TodoItems.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> TodoItemExists(int id)
    {
        return await _context.TodoItems.AnyAsync(e => e.Id == id);
    }
}
