using BaseRepository.Application.Abstractions;

namespace BaseRepository.Application.UnitTests.TestSupport;

public class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; }
}
