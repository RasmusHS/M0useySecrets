using System.Security.Cryptography;
using M0useySecrets.Core.Crypto;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Models;
using M0useySecrets.Core.Services.Interfaces;
using M0useySecrets.Core.Storage.Interfaces;

namespace M0useySecrets.Core.Services;

/// <summary>
/// The VaultService is responsible for managing the lifecycle of the vault, including initialization, unlocking, locking, and changing the master password.
/// </summary>
public class VaultService : IVaultService
{
    private readonly IVaultStore _store;
    private readonly IKeyDerivation _keyDerivation;
    private readonly IAesEncryptor _encryptor;
    private readonly IVaultPathResolver _pathResolver;

    private SecureKeyContext? _keyContext; // Null when locked
    private Vault? _vault; // Null when locked

    public VaultService(IVaultStore store, IKeyDerivation keyDerivation, IAesEncryptor encryptor, IVaultPathResolver pathResolver)
    {
        _store = store;
        _keyDerivation = keyDerivation;
        _encryptor = encryptor;
        _pathResolver = pathResolver;
    }

    // --- Init (first-time setup) ---

    /// <summary>
    /// Initializes a new vault with the given master password. 
    /// This should only be called once, and will throw if a vault already exists. 
    /// It generates a new salt, derives the KEK, creates an empty vault, and saves it to storage. 
    /// After this, the vault is unlocked and ready for use.
    /// </summary>
    /// <param name="masterPassword">The master password to initialize the vault with.</param>
    /// <exception cref="InvalidOperationException">Thrown if the vault is already initialized.</exception>
    public void Initialize(string masterPassword)
    {
        // 1. check _pathResolver.VaultExists() — if true, throw (already initialized)
        if (_pathResolver.VaultExists())
            throw new InvalidOperationException("Vault already initialized.");

        // 2. generate salt via _keyDerivation.GenerateSalt()
        byte[] salt = _keyDerivation.GenerateSalt();

        // 3. pick iteration count (const, e.g. 600_000)
        const int iterations = 600_000;

        // 4. derive KEK via _keyDerivation.DeriveKey(password, salt, iterations)
        byte[] kek = _keyDerivation.DeriveKey(masterPassword, salt, iterations);

        // 5. build a Vault with empty secrets list, salt, iterations
        Vault vault = new Vault
        {
            Salt = salt,
            Iterations = iterations,
            Secrets = new List<SecretEntry>()
        };

        // 6. _store.CreateVault(vault, kek)
        _store.CreateVault(vault, kek);

        // 7. store _keyContext = new SecureKeyContext(kek)
        _keyContext = new SecureKeyContext(kek);

        // 8. store _vault = vault
        _vault = vault;
    }

    // --- Unlock (open existing vault) ---

    /// <summary>
    /// Unlocks the vault with the given master password.
    /// </summary>
    /// <param name="masterPassword">The master password to unlock the vault with.</param>
    /// <exception cref="InvalidOperationException">Thrown if the vault cannot be unlocked with the provided password.</exception>
    public void Unlock(string masterPassword)
    {
        // 1. load header from store
        VaultHeader header = _store.LoadHeader();

        // 2. derive KEK from password + header.Salt + header.Iterations
        byte[] kek = _keyDerivation.DeriveKey(masterPassword, header.Salt, header.Iterations);
        //Console.WriteLine(kek);
        // 3. load vault with KEK
        _vault = _store.LoadVault(kek);

        // 4. create new _keyContext with KEK
        _keyContext = new SecureKeyContext(kek);
    }

    // --- Lock (close vault, wipe key from memory) ---

    /// <summary>
    /// Locks the vault by disposing the key context and setting both the key context and vault references to null.
    /// </summary>
    public void Lock()
    {
        // 1. dispose _keyContext
        _keyContext?.Dispose();

        // 2. set _keyContext to null
        _keyContext = null;

        // 3. set _vault to null
        _vault = null;
    }

    // --- Change master password ---

    /// <summary>
    /// Changes the master password by re-deriving the KEK with the new password and a new salt, 
    /// then re-wrapping all secrets' DEKs with the new KEK, and finally saving the updated vault.
    /// </summary>
    /// <param name="currentPassword">The current master password.</param>
    /// <param name="newPassword">The new master password.</param>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the current password is incorrect.</exception>
    public void ChangePassword(string currentPassword, string newPassword)
    {
        // 1. check if vault is unlocked (i.e. _vault and _keyContext are not null) — if not, throw
        if (_vault == null)
            throw new VaultLockedException("Vault is locked.");

        // 2. derive old KEK, verify it matches (safety check)
        byte[] oldKek = _keyDerivation.DeriveKey(currentPassword, _vault.Salt, _vault.Iterations);
        if (!CryptographicOperations.FixedTimeEquals(oldKek, _keyContext?.Key))
            throw new InvalidOperationException("Current password is incorrect.");

        // 3. generate new salt
        byte[] newSalt = _keyDerivation.GenerateSalt();

        // 4. derive new KEK from newPassword + new salt
        byte[] newKek = _keyDerivation.DeriveKey(newPassword, newSalt, _vault.Iterations);

        // 5. re-wrap every secret's DEK with the new KEK
        int secretCount = _vault.Secrets.Count;
        for (int i = 0; i < secretCount; i++)
        {
            SecretEntry secret = _vault.Secrets[i];

            // a. unwrap DEK with OLD kek
            byte[] dek = _encryptor.UnwrapKey(secret.WrappedDek, secret.DekNonce, secret.DekTag, oldKek);

            // b. wrap DEK with NEW kek
            EncryptionResult result = _encryptor.WrapKey(dek, newKek);

            // c. update secret's WrappedDek, DekNonce, DekTag
            secret.WrappedDek = result.Ciphertext; // Note: for wrapping, we can reuse the same properties since the ciphertext is the new wrapped key
            secret.DekNonce = result.Nonce;
            secret.DekTag = result.Tag;

            CryptographicOperations.ZeroMemory(dek); // Clear unwrapped DEK from memory
        }
        CryptographicOperations.ZeroMemory(oldKek); // Clear old KEK from memory

        // 6. update vault's salt and iterations (keep iterations same, just new salt)
        _vault.Salt = newSalt;

        // 7. save updated vault with new KEK
        _store.SaveVault(_vault, newKek);

        // 8. dispose old _keyContext
        _keyContext?.Dispose();

        // 9. create new _keyContext with new KEK
        _keyContext = new SecureKeyContext(newKek);
    }

    // --- Accessors for other services ---

    /// <summary>
    /// Gets the currently loaded vault, or throws if the vault is locked.
    /// </summary>
    /// <returns>The currently loaded vault.</returns>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    public Vault GetVaultOrThrow()
    {
        if (_vault == null)
            throw new VaultLockedException("Vault is locked.");

        return _vault;
    }

    /// <summary>
    /// Gets the current KEK from the key context, or throws if the vault is locked.
    /// </summary>
    /// <returns>The current KEK.</returns>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    public byte[] GetKeyOrThrow()
    {
        if (_keyContext == null)
            throw new VaultLockedException("Vault is locked.");

        return _keyContext.Key;
    }
}
