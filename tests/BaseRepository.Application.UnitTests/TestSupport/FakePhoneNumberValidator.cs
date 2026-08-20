using System;
using BaseRepository.Application.Abstractions;

namespace BaseRepository.Application.UnitTests.TestSupport;

/// <summary>
/// A minimal stand-in for the real libphonenumber-backed validator: only understands numbers
/// already in "+&lt;digits&gt;" form, which is all these tests need - the real validator's
/// actual per-country behavior is proven separately in
/// BaseRepository.Infrastructure.IntegrationTests.
/// </summary>
public class FakePhoneNumberValidator : IPhoneNumberValidator
{
    public bool IsValid(string phoneNumber, string? defaultRegion = null)
        => phoneNumber.StartsWith('+') && phoneNumber.Length > 1;

    public string ToE164(string phoneNumber, string? defaultRegion = null)
        => IsValid(phoneNumber, defaultRegion) ? phoneNumber : throw new ArgumentException("Not a valid phone number.", nameof(phoneNumber));
}
