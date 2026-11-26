using System.Globalization;
using System.Text;

namespace Infrastructure.Authentication;

internal static class PhoneNumberNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var digits = new StringBuilder(value.Length);
        var hasPlusPrefix = false;

        foreach (var character in value.Trim())
        {
            var digit = CharUnicodeInfo.GetDecimalDigitValue(character);
            if (digit >= 0)
            {
                digits.Append((char)('0' + digit));
            }
            else if (character == '+' && digits.Length == 0 && !hasPlusPrefix)
            {
                hasPlusPrefix = true;
            }
            else if (!char.IsWhiteSpace(character) &&
                     character is not ('-' or '(' or ')' or '.' or '/'))
            {
                return string.Empty;
            }
        }

        if (!hasPlusPrefix && digits.Length >= 2 && digits[0] == '0' && digits[1] == '0')
            digits.Remove(0, 2);

        return digits.ToString();
    }

    public static bool IsValid(string value) =>
        System.Text.RegularExpressions.Regex.IsMatch(value, @"^[1-9]\d{7,14}$");
}
