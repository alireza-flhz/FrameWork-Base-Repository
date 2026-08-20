using System;
using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Domain.UnitTests.Common;

public class IranianMobileNumberTests
{
    [Theory]
    [InlineData("09123456789")]
    [InlineData("+989123456789")]
    [InlineData("00989123456789")]
    [InlineData("989123456789")]
    [InlineData("9123456789")]
    public void IsValid_WithAKnownAcceptedForm_ReturnsTrue(string number)
    {
        Assert.True(IranianMobileNumber.IsValid(number));
    }

    [Theory]
    [InlineData("0812345678")]
    [InlineData("091234567")]
    [InlineData("091234567890")]
    [InlineData("not-a-number")]
    [InlineData(null)]
    public void IsValid_WithGarbageInput_ReturnsFalse(string? number)
    {
        Assert.False(IranianMobileNumber.IsValid(number));
    }

    [Theory]
    [InlineData("09123456789", "09123456789")]
    [InlineData("+989123456789", "09123456789")]
    [InlineData("00989123456789", "09123456789")]
    [InlineData("989123456789", "09123456789")]
    [InlineData("9123456789", "09123456789")]
    public void Normalize_AlwaysProducesTheLocalElevenDigitForm(string input, string expected)
    {
        Assert.Equal(expected, IranianMobileNumber.Normalize(input));
    }

    [Fact]
    public void Normalize_WithAnInvalidNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() => IranianMobileNumber.Normalize("not-a-number"));
    }
}
