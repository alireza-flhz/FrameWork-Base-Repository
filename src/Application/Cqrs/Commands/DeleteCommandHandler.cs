using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;

namespace BaseRepository.Application.Cqrs.Commands;

public class DeleteCommandHandler<TEntity, TKey> : IRequestHandler<DeleteCommand<TEntity, TKey>, Unit>
    where TEntity : BaseEntity<TKey>
{
    private readonly IRepository<TEntity, TKey> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommandHandler(IRepository<TEntity, TKey> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCommand<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, request.Id!);

        _repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
