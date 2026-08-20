using BaseRepository.Application.Abstractions;

namespace BaseRepository.Application.UnitTests.TestSupport;

/// <summary>
/// A trivial reversible "hash" (just prefixes the password) so Application-layer tests can
/// assert register/login behavior without pulling in a real hashing algorithm - that's
/// PasswordHasher's job, proven separately in BaseRepository.Infrastructure.IntegrationTests.
/// </summary>
public class FakePasswordHasher : IPasswordHasher
{
    private const string Prefix = "hashed:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string password, string passwordHash) => passwordHash == Prefix + password;
}
