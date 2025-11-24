using System.Globalization;
using System.Text;

namespace Cartesian.Services.Chat;

public static class EmojiValidator
{
    public static bool IsValidEmoji(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var normalized = input.Normalize(NormalizationForm.FormC);
        
        if (normalized.Length < 1 || normalized.Length > 20)
            return false;

        var textElements = StringInfo.GetTextElementEnumerator(normalized);
        
        var count = 0;
        while (textElements.MoveNext())
        {
            count++;
            if (count > 1)
                return false;
        }

        if (count != 1)
            return false;

        return ContainsEmojiCharacters(normalized);
    }

    private static bool ContainsEmojiCharacters(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            if (IsEmojiRune(rune))
                return true;
        }

        return false;
    }

    private static bool IsEmojiRune(Rune rune)
    {
        var value = rune.Value;
        
        return IsInRange(value, 0x1F300, 0x1F6FF) ||
               IsInRange(value, 0x1F900, 0x1F9FF) ||
               IsInRange(value, 0x1FA00, 0x1FA6F) ||
               IsInRange(value, 0x1FA70, 0x1FAFF) ||
               IsInRange(value, 0x2600, 0x26FF) ||
               IsInRange(value, 0x2700, 0x27BF) ||
               IsInRange(value, 0x1F1E6, 0x1F1FF) ||
               IsInRange(value, 0xFE00, 0xFE0F) ||
               IsInRange(value, 0x1F3FB, 0x1F3FF) ||
               value == 0x200D ||
               value == 0x20E3 ||
               IsInRange(value, 0x231A, 0x231B) ||
               IsInRange(value, 0x2328, 0x2328) ||
               IsInRange(value, 0x23CF, 0x23CF) ||
               IsInRange(value, 0x23E9, 0x23F3) ||
               IsInRange(value, 0x23F8, 0x23FA) ||
               IsInRange(value, 0x24C2, 0x24C2) ||
               IsInRange(value, 0x25AA, 0x25AB) ||
               IsInRange(value, 0x25B6, 0x25B6) ||
               IsInRange(value, 0x25C0, 0x25C0) ||
               IsInRange(value, 0x25FB, 0x25FE) ||
               IsInRange(value, 0x2614, 0x2615) ||
               IsInRange(value, 0x2648, 0x2653) ||
               IsInRange(value, 0x267F, 0x267F) ||
               IsInRange(value, 0x2693, 0x2693) ||
               IsInRange(value, 0x26A1, 0x26A1) ||
               IsInRange(value, 0x26AA, 0x26AB) ||
               IsInRange(value, 0x26BD, 0x26BE) ||
               IsInRange(value, 0x26C4, 0x26C5) ||
               IsInRange(value, 0x26CE, 0x26CE) ||
               IsInRange(value, 0x26D4, 0x26D4) ||
               IsInRange(value, 0x26EA, 0x26EA) ||
               IsInRange(value, 0x26F2, 0x26F3) ||
               IsInRange(value, 0x26F5, 0x26F5) ||
               IsInRange(value, 0x26FA, 0x26FA) ||
               IsInRange(value, 0x26FD, 0x26FD) ||
               IsInRange(value, 0x2702, 0x2702) ||
               IsInRange(value, 0x2705, 0x2705) ||
               IsInRange(value, 0x2708, 0x270D) ||
               IsInRange(value, 0x270F, 0x270F) ||
               IsInRange(value, 0x2712, 0x2712) ||
               IsInRange(value, 0x2714, 0x2714) ||
               IsInRange(value, 0x2716, 0x2716) ||
               IsInRange(value, 0x271D, 0x271D) ||
               IsInRange(value, 0x2721, 0x2721) ||
               IsInRange(value, 0x2728, 0x2728) ||
               IsInRange(value, 0x2733, 0x2734) ||
               IsInRange(value, 0x2744, 0x2744) ||
               IsInRange(value, 0x2747, 0x2747) ||
               IsInRange(value, 0x274C, 0x274C) ||
               IsInRange(value, 0x274E, 0x274E) ||
               IsInRange(value, 0x2753, 0x2755) ||
               IsInRange(value, 0x2757, 0x2757) ||
               IsInRange(value, 0x2763, 0x2764) ||
               IsInRange(value, 0x2795, 0x2797) ||
               IsInRange(value, 0x27A1, 0x27A1) ||
               IsInRange(value, 0x27B0, 0x27B0) ||
               IsInRange(value, 0x27BF, 0x27BF) ||
               IsInRange(value, 0x2934, 0x2935) ||
               IsInRange(value, 0x2B05, 0x2B07) ||
               IsInRange(value, 0x2B1B, 0x2B1C) ||
               IsInRange(value, 0x2B50, 0x2B50) ||
               IsInRange(value, 0x2B55, 0x2B55) ||
               IsInRange(value, 0x3030, 0x3030) ||
               IsInRange(value, 0x303D, 0x303D) ||
               IsInRange(value, 0x3297, 0x3297) ||
               IsInRange(value, 0x3299, 0x3299);
    }

    private static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    public static string NormalizeEmoji(string emoji)
    {
        return emoji.Normalize(NormalizationForm.FormC).Trim();
    }
}
