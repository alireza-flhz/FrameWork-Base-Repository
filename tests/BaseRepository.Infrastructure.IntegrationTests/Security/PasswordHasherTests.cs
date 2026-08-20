using BaseRepository.Infrastructure.Security;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_NeverReturnsThePlainTextPassword()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.NotEqual("correct-horse-battery-staple", hash);
    }

    [Fact]
    public void Verify_WithTheCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.True(_hasher.Verify("correct-horse-battery-staple", hash));
    }

    [Fact]
    public void Verify_WithTheWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_CalledTwiceForTheSamePassword_ProducesDifferentHashes()
    {
        // BCrypt salts each hash randomly, so two hashes of the same password should never
        // match byte-for-byte even though both verify correctly.
        var first = _hasher.Hash("correct-horse-battery-staple");
        var second = _hasher.Hash("correct-horse-battery-staple");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("correct-horse-battery-staple", first));
        Assert.True(_hasher.Verify("correct-horse-battery-staple", second));
    }
}
