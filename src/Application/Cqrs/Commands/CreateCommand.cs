using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Cqrs.Commands;

public class CreateCommand<TEntity, TKey, TDto> : IRequest<TDto>
    where TEntity : BaseEntity<TKey>
{
    public TDto Dto { get; }

    public CreateCommand(TDto dto)
    {
        Dto = dto;
    }
}
