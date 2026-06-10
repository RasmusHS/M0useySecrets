namespace M0useySecrets.Core.Crypto;

public class KeyDerivation //: IKeyDerivation
{
    private const int KeySizeBytes = 32;

    public byte[] GenerateSalt()
    {
        // RandomNumberGenerator.Fill() — NOT Random
    }

    public byte[] DeriveKey(string password, byte[] salt, int iterations)
    {
        // Rfc2898DeriveBytes with:
        //   - HashAlgorithmName.SHA256
        //   - output 32 bytes (= AES-256 key size)
        //   - iterations from parameter (store in metadata so you can bump it later)
        //
        // The password needs encoding to bytes first — use UTF8
    }

    public byte[] GenerateDek()
    {
        // RandomNumberGenerator.Fill — one fresh DEK per secret
        // called when ADDING a secret, not on every access
    }
}
