using BaseRepository.Infrastructure.Phone;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests.Phone;

/// <summary>
/// Proves the real libphonenumber-backed validator, not a fake - and that it's genuinely
/// international, not Iran-only (BaseRepository.Domain.Common.PersianMobileNumber already
/// covers the zero-dependency Iran-only case).
/// </summary>
public class PhoneNumberValidatorTests
{
    private readonly PhoneNumberValidator _validator = new();

    [Theory]
    [InlineData("+989123456789", null)] // Iran, international form
    [InlineData("09123456789", "IR")] // Iran, local form + region
    [InlineData("+16502530000", null)] // US (Google Mountain View), international form
    [InlineData("6502530000", "US")] // US, local form + region
    [InlineData("+442071838750", null)] // UK, international form
    [InlineData("02071838750", "GB")] // UK, local form + region
    [InlineData("+4930123456", null)] // Germany, international form
    [InlineData("+61491570156", null)] // Australia, international form
    public void IsValid_WithARealNumberFromAnyCountry_ReturnsTrue(string number, string? region)
    {
        Assert.True(_validator.IsValid(number, region));
    }

    [Theory]
    [InlineData("not-a-number", null)]
    [InlineData("12345", "US")]
    [InlineData("0912345", "IR")] // too short to be a real Iranian mobile number
    public void IsValid_WithGarbageOrImplausibleInput_ReturnsFalse(string number, string? region)
    {
        Assert.False(_validator.IsValid(number, region));
    }

    [Fact]
    public void IsValid_WithALocalFormatNumberAndNoRegion_ReturnsFalse()
    {
        // Without a region hint, a number with no leading "+" is ambiguous - correctly rejected
        // rather than guessed at.
        Assert.False(_validator.IsValid("09123456789", defaultRegion: null));
    }

    [Theory]
    [InlineData("09123456789", "IR", "+989123456789")]
    [InlineData("6502530000", "US", "+16502530000")]
    [InlineData("+442071838750", null, "+442071838750")]
    public void ToE164_NormalizesToTheInternationalForm(string number, string? region, string expectedE164)
    {
        Assert.Equal(expectedE164, _validator.ToE164(number, region));
    }

    [Fact]
    public void ToE164_WithAnInvalidNumber_ThrowsArgumentException()
    {
        Assert.Throws<System.ArgumentException>(() => _validator.ToE164("not-a-number"));
    }
}
