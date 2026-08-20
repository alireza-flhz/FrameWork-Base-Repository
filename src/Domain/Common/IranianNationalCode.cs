using System.Linq;

namespace BaseRepository.Domain.Common;

/// <summary>
/// Validates the checksum digit of an Iranian national ID (کد ملی) - 10 digits, the last one
/// a checksum over the first nine.
/// </summary>
public static class IranianNationalCode
{
    public static bool IsValid(string? nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10 || !nationalCode.All(char.IsDigit))
            return false;

        // A repeated-digit string (00000000000, 1111111111, ...) satisfies the checksum by
        // construction but is never a real national code - a common gap in naive implementations.
        if (nationalCode.Distinct().Count() == 1)
            return false;

        var digits = nationalCode.Select(c => c - '0').ToArray();
        var checkDigit = digits[9];

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += digits[i] * (10 - i);

        var remainder = sum % 11;
        var expected = remainder < 2 ? remainder : 11 - remainder;

        return checkDigit == expected;
    }
}
