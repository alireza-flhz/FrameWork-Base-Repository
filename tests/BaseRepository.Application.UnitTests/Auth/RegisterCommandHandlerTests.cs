using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Auth;
using BaseRepository.Application.UnitTests.TestSupport;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;
using Xunit;

namespace BaseRepository.Application.UnitTests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly InMemoryRepository<User, int> _repository = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();

    private RegisterCommandHandler CreateHandler()
        => new(_repository, _unitOfWork, _passwordHasher, _jwtTokenGenerator);

    [Fact]
    public async Task Handle_WithANewEmail_CreatesTheUserWithAHashedPasswordAndReturnsAToken()
    {
        var handler = CreateHandler();
        var command = new RegisterCommand { Email = "New@Example.com", Password = "correct-horse" };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("new@example.com", result.Email);
        Assert.Equal($"token-for-{result.UserId}", result.Token);

        var stored = await _repository.GetByIdAsync(result.UserId, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal("new@example.com", stored!.Email);
        Assert.NotEqual("correct-horse", stored.PasswordHash);
        Assert.True(_passwordHasher.Verify("correct-horse", stored.PasswordHash));
    }

    [Fact]
    public async Task Handle_WithAnEmailAlreadyRegistered_ThrowsConflictException()
    {
        var handler = CreateHandler();
        await handler.Handle(new RegisterCommand { Email = "dup@example.com", Password = "correct-horse" }, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new RegisterCommand { Email = "DUP@example.com", Password = "another-password" }, CancellationToken.None));
    }
}
