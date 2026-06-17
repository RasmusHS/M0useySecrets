using System.Security.Cryptography;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Crypto;

/// <summary>
/// Implements AES-GCM encryption and decryption for both value encryption (DEK encrypting secret values) and key wrapping (KEK encrypting DEKs).
/// </summary>
public class AesEncryptor : IAesEncryptor
{
    private const int NonceSizeBytes = 12;    // AES-GCM standard
    private const int TagSizeBytes = 16;      // 128-bit tag
    private const int KeySizeBytes = 32;      // AES-256
    private static readonly byte[] SentinelBytes = "M0USEY_VAULT_OK"u8.ToArray();

    // --- Value encryption (DEK encrypts a secret's plaintext) ---

    /// <summary>
    /// Encrypts a plaintext value using the provided DEK. 
    /// This method is used for encrypting the actual secret values, where the DEK is the key that will be stored (wrapped) in the vault. 
    /// It validates that the DEK is the correct size and then delegates to the common Encrypt method which handles AES-GCM encryption logic.
    /// </summary>
    /// <param name="plaintext">The plaintext value to encrypt.</param>
    /// <param name="dek">The Data Encryption Key (DEK) used for encryption.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the ciphertext, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the DEK is not the correct size.</exception>
    public EncryptionResult EncryptValue(byte[] plaintext, byte[] dek)
    {
        if (dek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(dek));

        return Encrypt(plaintext, dek);
    }

    /// <summary> 
    /// Used for both value encryption (DEK encrypts secret) and key wrapping (KEK encrypts DEK). 
    /// The "plaintext" is either the secret value or the DEK, and the "key" is either the DEK or the KEK, depending on context.
    /// This method encapsulates the common AES-GCM encryption logic, generating a random nonce and producing the ciphertext and authentication tag.
    /// </summary>
    /// <param name="plaintext_dek">The plaintext/DEK to encrypt.</param>
    /// <param name="dek_kek">The DEK/KEK to use for encryption.</param> 
    /// <returns>An <see cref="EncryptionResult"/> containing the ciphertext, nonce, and authentication tag.</returns> 
    private EncryptionResult Encrypt(byte[] plaintext_dek, byte[] dek_kek)
    {
        // 1. allocate nonce = new byte[NonceSizeBytes]
        byte[] nonce = new byte[NonceSizeBytes];

        // 2. RandomNumberGenerator.Fill(nonce)
        RandomNumberGenerator.Fill(nonce);

        // 3. allocate ciphertext = new byte[plaintext.Length]
        byte[] ciphertext = new byte[plaintext_dek.Length];

        // 4. allocate tag = new byte[TagSizeBytes]
        byte[] tag = new byte[TagSizeBytes];

        // 5. using var aes = new AesGcm(dek, TagSizeBytes)
        using var aes = new AesGcm(dek_kek, TagSizeBytes);

        // 6. aes.Encrypt(nonce, plaintext, ciphertext, tag)
        aes.Encrypt(nonce, plaintext_dek, ciphertext, tag);

        // 7. return new EncryptionResult(ciphertext, nonce, tag)
        return new EncryptionResult(ciphertext, nonce, tag);
    }

    /// <summary>
    /// Decrypts a ciphertext value using the provided DEK, nonce, and authentication tag.
    /// </summary>
    /// <param name="ciphertext">The ciphertext to decrypt.</param>
    /// <param name="nonce">The nonce used during encryption.</param>
    /// <param name="tag">The authentication tag from encryption.</param>
    /// <param name="dek">The Data Encryption Key (DEK) used for decryption.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="ArgumentException">Thrown if the DEK is not the correct size.</exception>
    public byte[] DecryptValue(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] dek)
    {
        if (dek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(dek));

        return Decrypt(ciphertext, nonce, tag, dek);
    }

    // --- Key wrapping (KEK encrypts a DEK) ---

    /// <summary>
    /// Wraps a Data Encryption Key (DEK) using a Key Encryption Key (KEK).
    /// </summary>
    /// <param name="dek">The DEK to wrap.</param>
    /// <param name="kek">The KEK used for wrapping.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the wrapped DEK, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception> 
    public EncryptionResult WrapKey(byte[] dek, byte[] kek)
    {
        if (kek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(kek));

        return Encrypt(dek, kek);
    }

    /// <summary>
    /// This method is used for both value decryption (DEK decrypts secret) and key unwrapping (KEK decrypts DEK). 
    /// The "ciphertext_wrappedDek" is either the secret value or the wrapped DEK, and the "dek_kek" is either the DEK or the KEK, depending on context.
    /// This method encapsulates the common AES-GCM decryption logic, using the provided nonce and authentication tag.
    /// </summary>
    /// <param name="ciphertext_wrappedDek">The ciphertext or wrapped DEK to decrypt.</param>
    /// <param name="nonce">The nonce used during encryption.</param>
    /// <param name="tag">The authentication tag from encryption.</param>
    /// <param name="dek_kek">The DEK or KEK used for decryption.</param>
    /// <returns>The decrypted plaintext or unwrapped key.</returns>
    private byte[] Decrypt(byte[] ciphertext_wrappedDek, byte[] nonce, byte[] tag, byte[] dek_kek)
    {
        // 1. allocate result = new byte[ciphertext_wrappedDek.Length]
        byte[] result = new byte[ciphertext_wrappedDek.Length];

        // 2. using var aes = new AesGcm(dek_kek, TagSizeBytes)
        using var aes = new AesGcm(dek_kek, TagSizeBytes);

        // 3. aes.Decrypt(nonce, ciphertext_wrappedDek, tag, result)
        //    → throws CryptographicException if tampered or wrong key
        aes.Decrypt(nonce, ciphertext_wrappedDek, tag, result);

        // return result to caller (either DecryptValue or UnwrapKey)
        return result;
    }

    /// <summary>
    /// Unwraps a wrapped DEK using the provided KEK, nonce, and authentication tag.
    /// </summary>
    /// <param name="wrappedDek">The wrapped DEK to unwrap.</param>
    /// <param name="nonce">The nonce used during wrapping.</param>
    /// <param name="tag">The authentication tag from wrapping.</param>
    /// <param name="kek">The Key Encryption Key (KEK) used for unwrapping.</param>
    /// <returns>The unwrapped DEK.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    public byte[] UnwrapKey(byte[] wrappedDek, byte[] nonce, byte[] tag, byte[] kek)
    {
        if (kek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(kek));

        return Decrypt(wrappedDek, nonce, tag, kek);
    }

    // --- Password verification sentinel ---

    /// <summary>
    /// Encrypts a known fixed value (sentinel) using the provided KEK. 
    /// This allows for password verification later by attempting to decrypt this sentinel and checking if it matches the expected value.
    /// </summary>
    /// <param name="kek">The Key Encryption Key (KEK) used for encryption.</param>
    /// <returns>An <see cref="EncryptionResult"/> containing the encrypted sentinel, nonce, and authentication tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    public EncryptionResult EncryptSentinel(byte[] kek)
    {
        if (kek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(kek));

        // encrypt a known fixed value so you can test the password later
        // the sentinel can be a hardcoded byte array, e.g. UTF8 bytes of "M0USEY_VAULT_OK"

        // 1. byte[] sentinel = Encoding.UTF8.GetBytes("M0USEY_VAULT_OK")
        byte[] sentinel = SentinelBytes;

        // 2. return EncryptValue(sentinel, kek)
        return EncryptValue(sentinel, kek);

    }

    /// <summary>
    /// Verifies the provided ciphertext, nonce, and tag against the expected sentinel value using the provided KEK.
    /// </summary>
    /// <param name="ciphertext">The encrypted sentinel value.</param>
    /// <param name="nonce">The nonce used during encryption.</param>
    /// <param name="tag">The authentication tag from encryption.</param>
    /// <param name="kek">The Key Encryption Key (KEK) used for decryption.</param>
    /// <returns>True if the sentinel matches the expected value, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if the KEK is not the correct size.</exception>
    public bool VerifySentinel(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] kek)
    {
        if (kek.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes", nameof(kek));

        // try to decrypt the sentinel — if it succeeds and matches, password is correct

        // 1. try: plaintext = DecryptValue(ciphertext, nonce, tag, kek)
        try
        {
            byte[] plaintext = DecryptValue(ciphertext, nonce, tag, kek);

            // 2. compare plaintext to expected sentinel bytes
            byte[] expected = SentinelBytes;

            // 3. return true if match
            return CryptographicOperations.FixedTimeEquals(plaintext, expected);
        }
        catch (CryptographicException)
        {
            // 4. catch CryptographicException → return false (wrong password)
            return false;
        }
    }
}
