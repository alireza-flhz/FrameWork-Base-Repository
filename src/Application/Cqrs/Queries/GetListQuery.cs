using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Cqrs.Queries;

public class GetListQuery<TEntity, TKey, TDto> : IRequest<PagedResult<TDto>>
    where TEntity : BaseEntity<TKey>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetListQuery(int pageIndex = 1, int pageSize = 20)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
