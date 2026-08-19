using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.UnitTests.TestSupport;

public class SampleEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;

    public SampleEntity()
    {
    }

    public SampleEntity(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
