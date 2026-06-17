namespace M0useySecrets.Core.Crypto.Interfaces;

public interface IKeyDerivation
{
    /// <summary>
    /// Generates a random salt of the appropriate size for key derivation.
    /// </summary>
    /// <returns>A byte array containing the generated salt.</returns>
    byte[] GenerateSalt();

    /// <summary>
    /// Derives a cryptographic key from the given password and salt using PBKDF2 with SHA-256.
    /// </summary>
    /// <param name="password">The password to derive the key from.</param>
    /// <param name="salt">The salt to use in the key derivation.</param>
    /// <param name="iterations">The number of iterations to perform in the key derivation.</param>
    /// <returns>A byte array containing the derived key.</returns>
    byte[] DeriveKey(string password, byte[] salt, int iterations);

    /// <summary>
    /// Generates a random Data Encryption Key (DEK) of the appropriate size.
    /// </summary>
    /// <returns>A byte array containing the generated DEK.</returns>
    byte[] GenerateDek();
}
