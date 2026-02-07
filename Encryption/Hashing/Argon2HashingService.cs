using Soenneker.Hashing.Argon2;

namespace Encryption.Hashing;

public class Argon2HashingService : IArgon2HashingService
{
    public async Task<string> Hash(string plainText)
    {
        return await Argon2HashingUtil.Hash(plainText);
    }

    public async Task<bool> Verify(string plainText, string hashedText)
    {
        return await Argon2HashingUtil.Verify(plainText, hashedText);
    }
}