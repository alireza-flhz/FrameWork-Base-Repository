using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;
using Mapster;

namespace BaseRepository.Application.Cqrs.Queries;

public class GetByIdQueryHandler<TEntity, TKey, TDto> : IRequestHandler<GetByIdQuery<TEntity, TKey, TDto>, TDto>
    where TEntity : BaseEntity<TKey>
{
    private readonly IRepository<TEntity, TKey> _repository;

    public GetByIdQueryHandler(IRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }

    public async Task<TDto> Handle(GetByIdQuery<TEntity, TKey, TDto> request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, request.Id!);

        return entity.Adapt<TDto>();
    }
}
