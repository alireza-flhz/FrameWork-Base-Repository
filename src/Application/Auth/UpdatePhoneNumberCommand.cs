using BaseRepository.Application.Messaging;

namespace BaseRepository.Application.Auth;

/// <summary>
/// The signed-in user's own phone number - the current user is resolved from the request's
/// claims (ICurrentUser), never from a caller-supplied id, so nobody can edit someone else's
/// profile this way. Pass null/empty PhoneNumber to clear a previously-set number.
///
/// Any country is accepted (via IPhoneNumberValidator - see its doc comment), not just Iran:
/// pass the number already in international form ("+98912...", "+1415...") and Region can stay
/// null, or pass a local-format number (e.g. "0912...") with Region set to its ISO 3166-1
/// alpha-2 code (e.g. "IR") so it's unambiguous which country's numbering plan applies.
/// </summary>
public class UpdatePhoneNumberCommand : IRequest<UserProfileDto>
{
    public string? PhoneNumber { get; set; }
    public string? Region { get; set; }
}
