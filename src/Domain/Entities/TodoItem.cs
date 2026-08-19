using System;

namespace BaseRepository.Domain.Entities;

/// <summary>
/// The template's example entity, wired all the way through Domain/Application/Infrastructure/
/// Api so a freshly scaffolded project runs and does something real out of the box. Once you've
/// added your own entities following this same pattern, delete this one (and TodoItemDto,
/// TodoItemsController, CreateTodoItemValidator) - it isn't part of the reusable base.
/// </summary>
public class TodoItem : BaseEntity<int>, IAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
