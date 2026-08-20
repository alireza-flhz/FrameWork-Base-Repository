using BaseRepository.Application.Messaging;

namespace BaseRepository.Application.Auth;

/// <summary>
/// The signed-in user's own phone number - the current user is resolved from the request's
/// claims (ICurrentUser), never from a caller-supplied id, so nobody can edit someone else's
/// profile this way. Pass null/empty to clear a previously-set number.
/// </summary>
public class UpdatePhoneNumberCommand : IRequest<UserProfileDto>
{
    public string? PhoneNumber { get; set; }
}
