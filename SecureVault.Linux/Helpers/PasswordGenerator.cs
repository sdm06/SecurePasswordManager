using System;
using System.Linq;
using System.Security.Cryptography;

namespace SecureVault.Linux.Helpers;

public class PasswordGenerator
{
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*()_-+=<>?";

    public static string Generate(int length = 16, bool useUpper = true, bool useDigits = true, bool useSpecial = true)
    {
        var charPool = Lower;
        if (useUpper) charPool += Upper;
        if (useDigits) charPool += Digits;
        if (useSpecial) charPool += Special;

        return RandomNumberGenerator.GetString(charPool, length);
    }
}