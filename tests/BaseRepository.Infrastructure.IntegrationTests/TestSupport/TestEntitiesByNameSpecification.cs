using BaseRepository.Application.Specifications;

namespace BaseRepository.Infrastructure.IntegrationTests.TestSupport;

public class TestEntitiesByNameSpecification : Specification<TestEntity>
{
    public TestEntitiesByNameSpecification(string nameContains, int? skip = null, int? take = null)
        : base(e => e.Name.Contains(nameContains))
    {
        ApplyOrderBy(e => e.Name);

        if (skip is not null && take is not null)
            ApplyPaging(skip.Value, take.Value);
    }
}
