using System;
using System.Globalization;

namespace BaseRepository.Domain.Common;

/// <summary>
/// Thin wrapper over the BCL's own <see cref="PersianCalendar"/> / <see cref="HijriCalendar"/>
/// (no extra dependency - both ship in .NET itself). Note HijriCalendar uses a fixed tabular
/// algorithm, not the official Umm al-Qura calendar, so it can be off by a day from the
/// observation-based calendar used for religious occasions.
/// </summary>
public static class IranianCalendar
{
    private static readonly PersianCalendar Persian = new();
    private static readonly HijriCalendar Hijri = new();

    public static (int Year, int Month, int Day) ToShamsi(DateTime gregorian)
        => (Persian.GetYear(gregorian), Persian.GetMonth(gregorian), Persian.GetDayOfMonth(gregorian));

    public static DateTime FromShamsi(int year, int month, int day)
        => Persian.ToDateTime(year, month, day, 0, 0, 0, 0);

    public static (int Year, int Month, int Day) ToHijri(DateTime gregorian)
        => (Hijri.GetYear(gregorian), Hijri.GetMonth(gregorian), Hijri.GetDayOfMonth(gregorian));

    public static DateTime FromHijri(int year, int month, int day)
        => Hijri.ToDateTime(year, month, day, 0, 0, 0, 0);
}
