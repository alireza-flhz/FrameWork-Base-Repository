namespace BaseRepository.Application.Abstractions;

/// <summary>
/// International phone number validation/normalization - any country, not just Iran (see
/// BaseRepository.Domain.Common.PersianMobileNumber for a zero-dependency Iran-only check).
/// <paramref name="defaultRegion"/> is an ISO 3166-1 alpha-2 code (e.g. "IR", "US", "DE") used
/// only to interpret a number with no leading "+"/country code; a number already in
/// international form ("+98...") ignores it.
/// </summary>
public interface IPhoneNumberValidator
{
    bool IsValid(string phoneNumber, string? defaultRegion = null);

    /// <summary>Normalizes to E.164 (e.g. "+989123456789"). Throws if not valid.</summary>
    string ToE164(string phoneNumber, string? defaultRegion = null);
}
