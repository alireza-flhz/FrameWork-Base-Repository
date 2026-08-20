using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Auth;
using BaseRepository.Application.UnitTests.TestSupport;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;
using Xunit;

namespace BaseRepository.Application.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly InMemoryRepository<User, int> _repository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();

    private LoginCommandHandler CreateHandler() => new(_repository, _passwordHasher, _jwtTokenGenerator);

    private async Task SeedUserAsync(string email, string password)
    {
        await _repository.AddAsync(new User { Email = email, PasswordHash = _passwordHasher.Hash(password) }, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WithCorrectCredentials_ReturnsAToken()
    {
        await SeedUserAsync("user@example.com", "correct-horse");
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand { Email = "USER@example.com", Password = "correct-horse" }, CancellationToken.None);

        Assert.Equal("user@example.com", result.Email);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task Handle_WithAnUnknownEmail_ThrowsAuthenticationFailedException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            handler.Handle(new LoginCommand { Email = "nobody@example.com", Password = "whatever" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithTheWrongPassword_ThrowsAuthenticationFailedException()
    {
        await SeedUserAsync("user@example.com", "correct-horse");
        var handler = CreateHandler();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            handler.Handle(new LoginCommand { Email = "user@example.com", Password = "wrong-password" }, CancellationToken.None));
    }
}
