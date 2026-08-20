using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Auth;
using BaseRepository.Application.UnitTests.TestSupport;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;
using Xunit;

namespace BaseRepository.Application.UnitTests.Auth;

public class UpdatePhoneNumberCommandHandlerTests
{
    private readonly InMemoryRepository<User, int> _repository = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();
    private readonly FakeCurrentUser _currentUser = new();

    private UpdatePhoneNumberCommandHandler CreateHandler() => new(_repository, _unitOfWork, _currentUser);

    private async Task<User> SeedUserAsync(int id, string email)
    {
        var user = new User { Email = email, PasswordHash = "irrelevant" };
        typeof(BaseEntity<int>).GetProperty(nameof(BaseEntity<int>.Id))!.SetValue(user, id);
        await _repository.AddAsync(user, CancellationToken.None);
        return user;
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ThrowsAuthenticationFailedException()
    {
        _currentUser.UserId = null;
        var handler = CreateHandler();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            handler.Handle(new UpdatePhoneNumberCommand { PhoneNumber = "09123456789" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SetsTheCurrentUsersPhoneNumber_NormalizedAndPersisted()
    {
        await SeedUserAsync(1, "user@example.com");
        _currentUser.UserId = 1;
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdatePhoneNumberCommand { PhoneNumber = "+989123456789" }, CancellationToken.None);

        Assert.Equal("09123456789", result.PhoneNumber);
        var stored = await _repository.GetByIdAsync(1, CancellationToken.None);
        Assert.Equal("09123456789", stored!.PhoneNumber);
    }

    [Fact]
    public async Task Handle_WithAnEmptyPhoneNumber_ClearsIt()
    {
        var user = await SeedUserAsync(1, "user@example.com");
        user.PhoneNumber = "09123456789";
        _currentUser.UserId = 1;
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdatePhoneNumberCommand { PhoneNumber = "" }, CancellationToken.None);

        Assert.Null(result.PhoneNumber);
    }

    [Fact]
    public async Task Handle_WithAPhoneNumberAlreadyUsedByAnotherUser_ThrowsConflictException()
    {
        var other = await SeedUserAsync(1, "other@example.com");
        other.PhoneNumber = "09123456789";
        await SeedUserAsync(2, "user@example.com");
        _currentUser.UserId = 2;
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdatePhoneNumberCommand { PhoneNumber = "09123456789" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReSettingTheCallersOwnCurrentPhoneNumber_DoesNotConflictWithItself()
    {
        var user = await SeedUserAsync(1, "user@example.com");
        user.PhoneNumber = "09123456789";
        _currentUser.UserId = 1;
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdatePhoneNumberCommand { PhoneNumber = "09123456789" }, CancellationToken.None);

        Assert.Equal("09123456789", result.PhoneNumber);
    }
}
