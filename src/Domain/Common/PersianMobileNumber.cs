using System;
using System.Text.RegularExpressions;

namespace BaseRepository.Domain.Common;

/// <summary>
/// Validates and normalizes an Iranian mobile number, accepting the common prefixed forms
/// (+98, 0098, 98, 0) and normalizing all of them to the local 11-digit form (09XXXXXXXXX).
/// </summary>
public static class PersianMobileNumber
{
    private static readonly Regex Pattern = new(@"^(?:\+98|0098|98|0)?(9\d{9})$", RegexOptions.Compiled);

    public static bool IsValid(string? mobileNumber) => mobileNumber is not null && Pattern.IsMatch(mobileNumber.Trim());

    public static string Normalize(string mobileNumber)
    {
        var match = Pattern.Match(mobileNumber.Trim());
        if (!match.Success)
            throw new ArgumentException("Not a valid Persian mobile number.", nameof(mobileNumber));

        return "0" + match.Groups[1].Value;
    }
}
