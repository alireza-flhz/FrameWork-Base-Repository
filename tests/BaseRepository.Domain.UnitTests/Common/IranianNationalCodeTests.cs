using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Domain.UnitTests.Common;

public class IranianNationalCodeTests
{
    // "127432724" + checksum: sum(digit[i] * (10-i)) = 182, 182 % 11 = 6, 6 >= 2 so
    // check digit = 11 - 6 = 5 -> "1274327245". Derived by hand from the algorithm, not
    // copied from the implementation under test.
    [Fact]
    public void IsValid_WithACorrectChecksum_ReturnsTrue()
    {
        Assert.True(IranianNationalCode.IsValid("1274327245"));
    }

    [Fact]
    public void IsValid_WithAWrongChecksumDigit_ReturnsFalse()
    {
        Assert.False(IranianNationalCode.IsValid("1274327240"));
    }

    [Theory]
    [InlineData("0000000000")]
    [InlineData("1111111111")]
    public void IsValid_WithAllDigitsTheSame_ReturnsFalse(string code)
    {
        Assert.False(IranianNationalCode.IsValid(code));
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("123456789012")]
    [InlineData("123456789a")]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_WithMalformedInput_ReturnsFalse(string? code)
    {
        Assert.False(IranianNationalCode.IsValid(code));
    }
}
