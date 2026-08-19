using BaseRepository.Application.Specifications;

namespace BaseRepository.Application.Cqrs.Queries;

public class PagedEntitySpecification<TEntity> : Specification<TEntity>
{
    public PagedEntitySpecification(int pageIndex, int pageSize)
    {
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
