using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using Mapster;

namespace BaseRepository.Application.Cqrs.Commands;

public class CreateCommandHandler<TEntity, TKey, TDto> : IRequestHandler<CreateCommand<TEntity, TKey, TDto>, TDto>
    where TEntity : BaseEntity<TKey>
{
    private readonly IRepository<TEntity, TKey> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommandHandler(IRepository<TEntity, TKey> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TDto> Handle(CreateCommand<TEntity, TKey, TDto> request, CancellationToken cancellationToken)
    {
        var entity = request.Dto.Adapt<TEntity>();

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Adapt<TDto>();
    }
}
