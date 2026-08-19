using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Cqrs.Commands;

public class DeleteCommand<TEntity, TKey> : IRequest<Unit>
    where TEntity : BaseEntity<TKey>
{
    public TKey Id { get; }

    public DeleteCommand(TKey id)
    {
        Id = id;
    }
}
