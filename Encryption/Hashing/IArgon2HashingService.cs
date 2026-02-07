namespace Encryption.Hashing;

public interface IArgon2HashingService
{
    Task<string> Hash(string plainText);
    
    Task<bool> Verify(string plainText, string hashedText);
}