using System.Security.Cryptography;
using M0useySecrets.Core.Crypto.Interfaces;

namespace M0useySecrets.Core.Crypto;

/// <summary>
/// Implements key derivation and generation logic for the vault.
/// </summary>
public class KeyDerivation : IKeyDerivation
{
    private const int KeySizeBytes = 32;

    /// <summary>
    /// Generates a random salt of the appropriate size for key derivation.
    /// </summary>
    /// <returns>A byte array containing the generated salt.</returns> 
    public byte[] GenerateSalt()
    {
        byte[] salt = new byte[KeySizeBytes];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    /// <summary>
    /// Derives a cryptographic key from the given password and salt using PBKDF2 with SHA-256.
    /// </summary>
    /// <param name="password">The password to derive the key from.</param>
    /// <param name="salt">The salt to use in the key derivation.</param>
    /// <param name="iterations">The number of iterations to perform in the key derivation.</param>
    /// <returns>A byte array containing the derived key.</returns>
    public byte[] DeriveKey(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySizeBytes);
    }

    /// <summary>
    /// Generates a random Data Encryption Key (DEK) of the appropriate size.
    /// </summary>
    /// <returns>A byte array containing the generated DEK.</returns>
    public byte[] GenerateDek()
    {
        byte[] dek = new byte[KeySizeBytes];
        RandomNumberGenerator.Fill(dek);
        return dek;
    }
}
