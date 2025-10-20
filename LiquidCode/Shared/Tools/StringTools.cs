using System.Security.Cryptography;
using System.Text;

namespace LiquidCode.Shared.Tools;

public static class StringTools
{
    public static string ComputeSha256(this string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static string RandomBase64(int byteSize)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(byteSize);
        return Convert.ToBase64String(randomBytes);
    }
}
