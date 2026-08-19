using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Cqrs.Queries;

public class GetByIdQuery<TEntity, TKey, TDto> : IRequest<TDto>
    where TEntity : BaseEntity<TKey>
{
    public TKey Id { get; }

    public GetByIdQuery(TKey id)
    {
        Id = id;
    }
}
