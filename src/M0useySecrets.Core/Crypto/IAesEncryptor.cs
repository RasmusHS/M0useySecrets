using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Crypto;

public interface IAesEncryptor
{
    /// <summary>
    /// Encrypts a plaintext value using the provided DEK. 
    /// This method is used for encrypting the actual secret values, where the DEK is the key that will be stored (wrapped) in the vault. 
    /// It validates that the DEK is the correct size and then delegates to the common Encrypt method which handles AES-GCM encryption logic.
    /// </summary>
    /// <param name="plaintext">The plaintext value to encrypt.</param>
    /// <param name="dek">The Data Encryption Key (DEK) used for encryption.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the ciphertext, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the DEK is not the correct size.</exception>
    EncryptionResult EncryptValue(byte[] plaintext, byte[] dek);

    /// <summary>
    /// Decrypts a ciphertext value using the provided DEK, nonce, and authentication tag.
    /// </summary>
    /// <param name="ciphertext">The ciphertext to decrypt.</param>
    /// <param name="nonce">The nonce used during encryption.</param>
    /// <param name="tag">The authentication tag from encryption.</param>
    /// <param name="dek">The Data Encryption Key (DEK) used for decryption.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="ArgumentException">Thrown if the DEK is not the correct size.</exception>
    byte[] DecryptValue(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] dek);

    /// <summary>
    /// Wraps a Data Encryption Key (DEK) using a Key Encryption Key (KEK).
    /// </summary>
    /// <param name="dek">The DEK to wrap.</param>
    /// <param name="kek">The KEK used for wrapping.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the wrapped DEK, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception> 
    EncryptionResult WrapKey(byte[] dek, byte[] kek);

    /// <summary>
    /// Unwraps a wrapped DEK using the provided KEK, nonce, and authentication tag.
    /// </summary>
    /// <param name="wrappedDek">The wrapped DEK to unwrap.</param>
    /// <param name="nonce">The nonce used during wrapping.</param>
    /// <param name="tag">The authentication tag from wrapping.</param>
    /// <param name="kek">The Key Encryption Key (KEK) used for unwrapping.</param>
    /// <returns>The unwrapped DEK.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    byte[] UnwrapKey(byte[] wrappedDek, byte[] nonce, byte[] tag, byte[] kek);

    /// <summary>
    /// Encrypts a known fixed value (sentinel) using the provided KEK. 
    /// This allows for password verification later by attempting to decrypt this sentinel and checking if it matches the expected value.
    /// </summary>
    /// <param name="kek">The Key Encryption Key (KEK) used for encryption.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the encrypted sentinel, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    EncryptionResult EncryptSentinel(byte[] kek);

    /// <summary>
    /// Verifies the provided ciphertext, nonce, and tag against the expected sentinel value using the provided KEK.
    /// </summary>
    /// <param name="ciphertext">The encrypted sentinel value.</param>
    /// <param name="nonce">The nonce used during encryption.</param>
    /// <param name="tag">The authentication tag from encryption.</param>
    /// <param name="kek">The Key Encryption Key (KEK) used for decryption.</param>
    /// <returns>True if the sentinel matches the expected value, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    bool VerifySentinel(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] kek);
}
