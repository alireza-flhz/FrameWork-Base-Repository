using BaseRepository.Application.Cqrs;

namespace BaseRepository.Application.TodoItems;

public class TodoItemDto : IHasId<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
