using System.Security.Cryptography;
using System.Text.Json;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Models;
using M0useySecrets.Core.Storage.Interfaces;

namespace M0useySecrets.Core.Storage;

/// <summary>
/// Handles all vault file read/write operations, including encryption and decryption.
/// </summary>
public class VaultStore : IVaultStore
{
    private readonly IAesEncryptor _encryptor;
    private readonly IVaultPathResolver _pathResolver;

    public VaultStore(IAesEncryptor encryptor, IVaultPathResolver pathResolver)
    {
        _encryptor = encryptor;
        _pathResolver = pathResolver;
    }

    // --- Vault initialization ---

    /// <summary>
    /// Creates a new vault file with the given KEK. The vault will be initialized with an empty secrets list.
    /// </summary>
    /// <param name="vault">The vault object to initialize.</param>
    /// <param name="kek">The key encryption key (KEK) used to encrypt the vault.</param>
    public void CreateVault(Vault vault, byte[] kek)
    {
        // just calls Save with an empty list
        SaveVault(vault, kek);
    }

    // --- Writing ---

    /// <summary>
    /// Saves the given vault to disk, encrypting it with the provided KEK. 
    /// This method performs an atomic write by first writing to a temporary file and then renaming it.
    /// </summary>
    /// <param name="vault">The vault object to save.</param>
    /// <param name="kek">The key encryption key (KEK) used to encrypt the vault.</param>
    public void SaveVault(Vault vault, byte[] kek)
    {
        // 1. serialize the secrets list to JSON bytes
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(vault.Secrets);

        // 2. encrypt that JSON blob with the KEK
        EncryptionResult payload = _encryptor.EncryptValue(jsonBytes, kek);
        CryptographicOperations.ZeroMemory(jsonBytes); // zero out plaintext JSON in memory immediately after use

        // 3. build the password check sentinel
        EncryptionResult sentinel = _encryptor.EncryptSentinel(kek);

        // 4. assemble the VaultFile object
        VaultFile file = new VaultFile
        {
            FormatVersion = 1,
            Salt = vault.Salt,
            Iterations = vault.Iterations,
            PasswordCheck = sentinel.Ciphertext,
            PasswordCheckNonce = sentinel.Nonce,
            PasswordCheckTag = sentinel.Tag,
            EncryptedPayload = payload.Ciphertext,
            PayloadNonce = payload.Nonce,
            PayloadTag = payload.Tag
        };

        // 5. serialize VaultFile to JSON
        byte[] fileBytes = JsonSerializer.SerializeToUtf8Bytes(file);

        // 6. atomic write
        string path = _pathResolver.GetVaultPath();
        string tempPath = path + ".tmp";
        _pathResolver.EnsureDirectoryExists();
        File.WriteAllBytes(tempPath, fileBytes);
        File.Move(tempPath, path, overwrite: true);
    }

    // --- Reading ---

    /// <summary>
    /// Loads just the vault header (salt and iterations) from disk without attempting to decrypt the secrets.
    /// </summary>
    /// <returns>The vault header containing the salt and iteration count.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the vault file is corrupt or empty.</exception>
    public VaultHeader LoadHeader()
    {
        // reads the vault file
        string path = _pathResolver.GetVaultPath();
        byte[] fileBytes = File.ReadAllBytes(path);

        // deserializes the outer VaultFile JSON
        VaultFile file = JsonSerializer.Deserialize<VaultFile>(fileBytes)
            ?? throw new InvalidOperationException("Vault file is corrupt or empty.");

        // returns just the plaintext fields — no decryption involved
        return new VaultHeader(file.Salt, file.Iterations);
    }

    /// <summary>
    /// Loads the entire vault from disk, decrypting it with the provided KEK.
    /// </summary>
    /// <param name="kek">The key encryption key (KEK) used to decrypt the vault.</param>
    /// <returns>The decrypted vault containing the secrets and metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the vault file is corrupt or empty.</exception>
    /// <exception cref="InvalidPasswordException">Thrown if the provided KEK is invalid.</exception>
    public Vault LoadVault(byte[] kek)
    {
        // 1. read file from disk
        string path = _pathResolver.GetVaultPath();
        byte[] fileBytes = File.ReadAllBytes(path);

        // 2. deserialize the outer VaultFile
        VaultFile file = JsonSerializer.Deserialize<VaultFile>(fileBytes)
            ?? throw new InvalidOperationException("Vault file is corrupt or empty.");

        // 3. verify master password via sentinel
        bool valid = _encryptor.VerifySentinel(file.PasswordCheck, file.PasswordCheckNonce, file.PasswordCheckTag, kek);
        if (!valid)
            throw new InvalidPasswordException();

        // 4. decrypt the payload
        byte[] jsonBytes = _encryptor.DecryptValue(file.EncryptedPayload, file.PayloadNonce, file.PayloadTag, kek);

        // 5. deserialize the secrets list
        List<SecretEntry> secrets = JsonSerializer.Deserialize<List<SecretEntry>>(jsonBytes)
            ?? throw new InvalidOperationException("Vault file is corrupt or empty.");

        // 6. return secrets + metadata needed for re-saving
        return new Vault
        {
            Salt = file.Salt,
            Iterations = file.Iterations,
            Secrets = secrets
        };
    }
}
