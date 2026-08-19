using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Cqrs.Commands;

public class UpdateCommand<TEntity, TKey, TDto> : IRequest<TDto>
    where TEntity : BaseEntity<TKey>
{
    public TKey Id { get; }
    public TDto Dto { get; }

    public UpdateCommand(TKey id, TDto dto)
    {
        Id = id;
        Dto = dto;
    }
}
