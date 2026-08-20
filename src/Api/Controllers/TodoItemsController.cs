using BaseRepository.Application.Messaging;
using BaseRepository.Application.TodoItems;
using BaseRepository.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BaseRepository.Api.Controllers;

/// <summary>
/// The template's example controller. Delete it (and TodoItem/TodoItemDto/AppDbContext's
/// TodoItems set/CreateTodoItemValidator) once you no longer need the example - or keep it as
/// a reference while you add your own entities the same way.
/// </summary>
[Route("api/v{version:apiVersion}/todo-items")]
public class TodoItemsController : BaseCrudController<TodoItem, int, TodoItemDto>
{
    public TodoItemsController(ISender sender) : base(sender)
    {
    }
}
