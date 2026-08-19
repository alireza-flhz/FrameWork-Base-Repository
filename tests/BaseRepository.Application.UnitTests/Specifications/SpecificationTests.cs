using BaseRepository.Application.Specifications;
using Xunit;

namespace BaseRepository.Application.UnitTests.Specifications;

public class SpecificationTests
{
    private sealed class SampleEntity
    {
        public int Age { get; set; }
    }

    private sealed class SampleSpecification : Specification<SampleEntity>
    {
        public SampleSpecification(int minAge)
        {
            AddCriteria(e => e.Age >= minAge);
            ApplyOrderByDescending(e => e.Age);
            ApplyPaging(skip: 5, take: 10);
        }
    }

    [Fact]
    public void ProtectedBuilderMethods_PopulateTheSpecification()
    {
        var spec = new SampleSpecification(minAge: 18);

        Assert.NotNull(spec.Criteria);
        Assert.NotNull(spec.OrderByDescending);
        Assert.Null(spec.OrderBy);
        Assert.True(spec.IsPagingEnabled);
        Assert.Equal(5, spec.Skip);
        Assert.Equal(10, spec.Take);
    }

    [Fact]
    public void Criteria_CorrectlyFiltersWhenCompiledAndRunInMemory()
    {
        var spec = new SampleSpecification(minAge: 18);
        var predicate = spec.Criteria!.Compile();

        Assert.True(predicate(new SampleEntity { Age = 20 }));
        Assert.False(predicate(new SampleEntity { Age = 10 }));
    }

    [Fact]
    public void AsNoTracking_DefaultsToTrue()
    {
        var spec = new SampleSpecification(minAge: 18);

        Assert.True(spec.AsNoTracking);
    }
}
