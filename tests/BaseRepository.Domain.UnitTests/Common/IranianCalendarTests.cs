using System;
using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Domain.UnitTests.Common;

public class IranianCalendarTests
{
    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2024, 3, 20)]
    [InlineData(1999, 12, 31)]
    [InlineData(2050, 6, 15)]
    public void ShamsiRoundTrip_ReturnsTheOriginalGregorianDate(int year, int month, int day)
    {
        var gregorian = new DateTime(year, month, day);

        var (shamsiYear, shamsiMonth, shamsiDay) = IranianCalendar.ToShamsi(gregorian);
        var roundTripped = IranianCalendar.FromShamsi(shamsiYear, shamsiMonth, shamsiDay);

        Assert.Equal(gregorian.Date, roundTripped.Date);
        Assert.InRange(shamsiMonth, 1, 12);
        Assert.InRange(shamsiDay, 1, 31);
    }

    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2024, 3, 20)]
    [InlineData(1999, 12, 31)]
    [InlineData(2050, 6, 15)]
    public void HijriRoundTrip_ReturnsTheOriginalGregorianDate(int year, int month, int day)
    {
        var gregorian = new DateTime(year, month, day);

        var (hijriYear, hijriMonth, hijriDay) = IranianCalendar.ToHijri(gregorian);
        var roundTripped = IranianCalendar.FromHijri(hijriYear, hijriMonth, hijriDay);

        Assert.Equal(gregorian.Date, roundTripped.Date);
        Assert.InRange(hijriMonth, 1, 12);
        Assert.InRange(hijriDay, 1, 30);
    }
}
