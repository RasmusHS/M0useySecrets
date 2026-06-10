namespace M0useySecrets.Core.Crypto;

public class AesEncryptor
{
    private const int NonceSizeBytes = 12;    // AES-GCM standard
    private const int TagSizeBytes = 16;      // 128-bit tag
    private const int KeySizeBytes = 32;      // AES-256

    public record EncryptionResult(byte[] Ciphertext, byte[] Nonce, byte[] Tag);

    // --- Value encryption (DEK encrypts a secret's plaintext) ---

    public EncryptionResult EncryptValue(byte[] plaintext, byte[] dek)
    {
        // AesGcm.Encrypt(nonce, plaintext, ciphertext, tag)
        // Generate nonce with RandomNumberGenerator — NEVER reuse a nonce with the same key

        // 1. allocate nonce = new byte[NonceSizeBytes]
        // 2. RandomNumberGenerator.Fill(nonce)
        // 3. allocate ciphertext = new byte[plaintext.Length]
        // 4. allocate tag = new byte[TagSizeBytes]
        // 5. using var aes = new AesGcm(dek, TagSizeBytes)
        // 6. aes.Encrypt(nonce, plaintext, ciphertext, tag)
        // 7. return new EncryptionResult(ciphertext, nonce, tag)
    }

    public byte[] DecryptValue(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] dek)
    {
        // AesGcm.Decrypt(nonce, ciphertext, tag, plaintext)
        // Throws CryptographicException on tamper — that's your "wrong password or corrupt" signal

        // 1. allocate plaintext = new byte[ciphertext.Length]
        // 2. using var aes = new AesGcm(dek, TagSizeBytes)
        // 3. aes.Decrypt(nonce, ciphertext, tag, plaintext)
        //    → throws CryptographicException if tampered or wrong key
        // 4. return plaintext
    }

    // --- Key wrapping (KEK encrypts a DEK) ---

    public EncryptionResult WrapKey(byte[] dek, byte[] kek)
    {
        // mechanically identical to EncryptValue
        // the "plaintext" is the DEK, the "key" is the KEK
        // 1. allocate nonce, ciphertext (dek.Length), tag
        // 2. RandomNumberGenerator.Fill(nonce)
        // 3. using var aes = new AesGcm(kek, TagSizeBytes)
        // 4. aes.Encrypt(nonce, dek, ciphertext, tag)
        // 5. return new EncryptionResult(ciphertext, nonce, tag)
    }

    public byte[] UnwrapKey(byte[] wrappedDek, byte[] nonce, byte[] tag, byte[] kek)
    {
        // mirror of WrapKey
        // 1. allocate dek = new byte[wrappedDek.Length]
        // 2. using var aes = new AesGcm(kek, TagSizeBytes)
        // 3. aes.Decrypt(nonce, wrappedDek, tag, dek)
        // 4. return dek
    }

    // --- Password verification sentinel ---

    public EncryptionResult EncryptSentinel(byte[] kek)
    {
        // encrypt a known fixed value so you can test the password later
        // the sentinel can be a hardcoded byte array, e.g. UTF8 bytes of "M0USEY_VAULT_OK"
        // 1. byte[] sentinel = Encoding.UTF8.GetBytes("M0USEY_VAULT_OK")
        // 2. return EncryptValue(sentinel, kek)
    }

    public bool VerifySentinel(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] kek)
    {
        // try to decrypt the sentinel — if it succeeds and matches, password is correct
        // 1. try: plaintext = DecryptValue(ciphertext, nonce, tag, kek)
        // 2. compare plaintext to expected sentinel bytes
        // 3. return true if match
        // 4. catch CryptographicException → return false (wrong password)
    }

    // private method the others delegate to
}
