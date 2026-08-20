using BaseRepository.Application.Cqrs.Commands;
using BaseRepository.Domain.Entities;
using FluentValidation;

namespace BaseRepository.Application.TodoItems;

public class CreateTodoItemValidator : AbstractValidator<CreateCommand<TodoItem, int, TodoItemDto>>
{
    public CreateTodoItemValidator()
    {
        RuleFor(x => x.Dto.Title).NotEmpty().MaximumLength(200);
    }
}
