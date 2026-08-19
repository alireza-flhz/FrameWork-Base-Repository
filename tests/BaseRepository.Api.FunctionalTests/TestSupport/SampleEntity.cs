using BaseRepository.Domain.Entities;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

public class SampleEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
}
