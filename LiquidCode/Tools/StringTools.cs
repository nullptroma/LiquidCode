using System.Security.Cryptography;
using System.Text;

namespace LiquidCode.Tools;

public static class StringTools
{
    private static readonly Random Random = new();

    public static string RandomBase64(int bytesCount)
    {
        var bytes = new byte[bytesCount];
        Random.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static string GuidBase64()
    {
        var g = Guid.NewGuid();
        return Convert.ToBase64String(g.ToByteArray());
    }

    public static string ComputeSha256(this string str)
    {
        using var sha256Hash = SHA256.Create();
        return GetHash(sha256Hash, str);
    }

    private static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        // Convert the input string to a byte array and compute the hash.
        var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (var i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }
}