using System;
using BaseRepository.Application.Abstractions;
using PhoneNumbers;

namespace BaseRepository.Infrastructure.Phone;

/// <summary>
/// Wraps Google's libphonenumber (via the libphonenumber-csharp port) - the same phone
/// validation engine behind Android's dialer - so every country's numbering plan is handled
/// for real instead of a hand-rolled per-country regex.
/// </summary>
public class PhoneNumberValidator : IPhoneNumberValidator
{
    private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public bool IsValid(string phoneNumber, string? defaultRegion = null)
    {
        try
        {
            var parsed = _phoneNumberUtil.Parse(phoneNumber, defaultRegion);
            return _phoneNumberUtil.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public string ToE164(string phoneNumber, string? defaultRegion = null)
    {
        PhoneNumber parsed;
        try
        {
            parsed = _phoneNumberUtil.Parse(phoneNumber, defaultRegion);
        }
        catch (NumberParseException ex)
        {
            throw new ArgumentException("Not a valid phone number.", nameof(phoneNumber), ex);
        }

        if (!_phoneNumberUtil.IsValidNumber(parsed))
            throw new ArgumentException("Not a valid phone number.", nameof(phoneNumber));

        return _phoneNumberUtil.Format(parsed, PhoneNumberFormat.E164);
    }
}
