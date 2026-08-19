using BaseRepository.Application.Cqrs;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

public class SampleDto : IHasId<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
