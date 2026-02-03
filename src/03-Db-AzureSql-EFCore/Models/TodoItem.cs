using System.ComponentModel.DataAnnotations;

namespace DbAzureSqlEFCore.Models;

/// <summary>
/// Todo item entity.
/// </summary>
public class TodoItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
