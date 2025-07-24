using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class AccountNicknameSpecification : ISpecification<string>
{
    public string ErrorMessage { get; private set; }

    // ±İÄ¢¾î ¸®½ºÆ®
    private static readonly string[] BannedWords = { "¹Ùº¸", "¸ÛÃ»ÀÌ", "¿î¿µÀÚ", "±èÈ«ÀÏ" };

    // ´Ğ³×ÀÓ Á¤±Ô½Ä
    private static readonly Regex NickNameRegex = new Regex(@"^[°¡-ÆRa-zA-Z]{2,7}$");

    public bool IsSatisfiedBy(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ErrorMessage = "´Ğ³×ÀÓÀº ºñ¾îÀÖÀ» ¼ö ¾ø½À´Ï´Ù.";
            return false;
        }

        if (!NickNameRegex.IsMatch(value))
        {
            ErrorMessage = "´Ğ³×ÀÓÀº ÇÑ±Û ¶Ç´Â ¿µ¹®À¸·Î 2ÀÚ ÀÌ»ó 7ÀÚ ÀÌÇÏ¸¸ °¡´ÉÇÕ´Ï´Ù.";
            return false;
        }

        foreach (var banned in BannedWords)
        {
            if (value.Contains(banned, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "´Ğ³×ÀÓ¿¡ ºÎÀûÀıÇÑ ´Ü¾î°¡ Æ÷ÇÔµÇ¾î ÀÖ½À´Ï´Ù.";
                return false;
            }
        }

        return true;
    }
}
