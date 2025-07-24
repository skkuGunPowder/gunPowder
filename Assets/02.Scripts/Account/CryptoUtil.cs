using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class CryptoUtil
{
    public static string Encryption(string plainText, string salt = "")
    {
        SHA256 sha256 = SHA256.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(plainText + salt);
        byte[] hash = sha256.ComputeHash(bytes);

        string resultText = string.Empty;
        foreach (byte b in hash) resultText += b.ToString("X2");

        return resultText;
    }

    public static bool Verify(string plainText, string hash, string salt = "")
    {
        return Encryption(plainText, salt) == hash;
    }
}
