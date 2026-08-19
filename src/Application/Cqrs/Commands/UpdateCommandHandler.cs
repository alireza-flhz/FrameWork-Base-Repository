using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;
using Mapster;

namespace BaseRepository.Application.Cqrs.Commands;

public class UpdateCommandHandler<TEntity, TKey, TDto> : IRequestHandler<UpdateCommand<TEntity, TKey, TDto>, TDto>
    where TEntity : BaseEntity<TKey>
{
    private readonly IRepository<TEntity, TKey> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommandHandler(IRepository<TEntity, TKey> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TDto> Handle(UpdateCommand<TEntity, TKey, TDto> request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, request.Id!);

        request.Dto.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Adapt<TDto>();
    }
}
