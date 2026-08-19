using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;
using Mapster;

namespace BaseRepository.Application.Cqrs.Queries;

public class GetListQueryHandler<TEntity, TKey, TDto> : IRequestHandler<GetListQuery<TEntity, TKey, TDto>, PagedResult<TDto>>
    where TEntity : BaseEntity<TKey>
{
    private readonly IRepository<TEntity, TKey> _repository;

    public GetListQueryHandler(IRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<TDto>> Handle(GetListQuery<TEntity, TKey, TDto> request, CancellationToken cancellationToken)
    {
        var specification = new PagedEntitySpecification<TEntity>(request.PageIndex, request.PageSize);
        var page = await _repository.PaginatedListAsync(specification, cancellationToken);

        var items = page.Items.Select(entity => entity.Adapt<TDto>()).ToList();

        return new PagedResult<TDto>(items, page.TotalCount, page.PageIndex, page.PageSize);
    }
}
