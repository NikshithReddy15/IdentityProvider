using System.Security.Cryptography;
using System.Text;

namespace Encryption.Hashing.SHA256Hashing;

public class SHA256HashingService : ISHA256HashingService
{
    public string Hash(string plainText)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(bytes);
    }
}