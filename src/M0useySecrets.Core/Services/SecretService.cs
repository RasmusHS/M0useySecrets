using System.Security.Cryptography;
using System.Text;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Models;
using M0useySecrets.Core.Services.Interfaces;
using M0useySecrets.Core.Storage.Interfaces;

namespace M0useySecrets.Core.Services;

/// <summary>
/// Service responsible for managing secrets within the vault, including adding, retrieving, listing, updating, and removing secrets.
/// </summary>
public class SecretService : ISecretService
{
    private readonly IVaultService _vaultService;
    private readonly IVaultStore _store;
    private readonly IAesEncryptor _encryptor;
    private readonly IKeyDerivation _keyDerivation;

    public SecretService(IVaultService vaultService, IVaultStore store, IAesEncryptor encryptor, IKeyDerivation keyDerivation)
    {
        _vaultService = vaultService;
        _store = store;
        _encryptor = encryptor;
        _keyDerivation = keyDerivation;
    }

    /// <summary>
    /// Adds a new secret to the vault. 
    /// The secret value is encrypted with a randomly generated DEK, which is then wrapped with the KEK. 
    /// The method checks for duplicate names within the same namespace and throws an exception if a conflict is found. 
    /// After adding the secret, the vault is saved to persistent storage.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <param name="expiresAt">The expiration date of the secret.</param>
    /// <param name="notes">Additional notes for the secret.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddSecret(string name, string value, string ns = "default", DateTime? expiresAt = null, string? notes = null)
    {
        // 1. get the vault:
        Vault vault = _vaultService.GetVaultOrThrow();

        // 2. check for duplicate: same name + namespace → throw
        if (vault.Secrets.Any(s => s.Name == name && s.Namespace == ns))
            throw new InvalidOperationException($"A secret with the name '{name}' already exists in the namespace '{ns}'.");

        // 3. generate a fresh DEK
        byte[] dek = _keyDerivation.GenerateDek();

        // 4. encrypt the value with the DEK
        EncryptionResult payload = _encryptor.EncryptValue(Encoding.UTF8.GetBytes(value), dek);

        // 5. wrap the DEK with the KEK
        EncryptionResult wrappedKey = _encryptor.WrapKey(dek, _vaultService.GetKeyOrThrow());

        // 6. zero the raw DEK immediately
        CryptographicOperations.ZeroMemory(dek);

        // 7. build SecretEntry with all fields
        SecretEntry secret = new SecretEntry
        {
            Id = Guid.NewGuid(),
            Name = name,
            Namespace = ns,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            Notes = notes,
            EncryptedValue = payload.Ciphertext,
            Nonce = payload.Nonce,
            Tag = payload.Tag,
            WrappedDek = wrappedKey.Ciphertext,
            DekNonce = wrappedKey.Nonce,
            DekTag = wrappedKey.Tag
        };

        // 8. add the secret to the vault
        vault.Secrets.Add(secret);

        // 9. save the vault
        _store.SaveVault(vault, _vaultService.GetKeyOrThrow());
    }

    /// <summary>
    /// Retrieves and decrypts a secret by its name and namespace.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <returns>The decrypted secret.</returns>
    /// <exception cref="SecretNotFoundException"></exception>
    public DecryptedSecret GetSecret(string name, string ns = "default")
    {
        // 1. get vault
        Vault vault = _vaultService.GetVaultOrThrow();

        // 2. find entry by name + namespace → throw SecretNotFoundException if missing
        SecretEntry entry = vault.Secrets.FirstOrDefault(s => s.Name == name && s.Namespace == ns)
            ?? throw new SecretNotFoundException();

        // 3. unwrap DEK
        byte[] dek = _encryptor.UnwrapKey(entry.WrappedDek, entry.DekNonce, entry.DekTag, _vaultService.GetKeyOrThrow());

        // 4. decrypt value
        byte[] plainBytes = _encryptor.DecryptValue(entry.EncryptedValue, entry.Nonce, entry.Tag, dek);

        // 5. zero the DEK
        CryptographicOperations.ZeroMemory(dek);

        // 6. map to DecryptedSecret with Value
        return new DecryptedSecret
        {
            Id = entry.Id,
            Name = entry.Name,
            Namespace = entry.Namespace,
            Value = Encoding.UTF8.GetString(plainBytes),
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt,
            ExpiresAt = entry.ExpiresAt,
            Notes = entry.Notes
        };
    }

    /// <summary>
    /// Lists all secrets in the vault, optionally filtered by namespace.
    /// </summary>
    /// <param name="ns">The namespace to filter secrets by. If null, all secrets are returned.</param>
    /// <returns>A list of decrypted secrets with their values set to null.</returns>
    public List<DecryptedSecret> ListSecrets(string? ns = null)
    {
        // 1. get vault
        Vault vault = _vaultService.GetVaultOrThrow();

        // 2. filter by namespace if provided, otherwise return all
        IEnumerable<SecretEntry> entries = ns == null ? vault.Secrets : vault.Secrets.Where(s => s.Namespace == ns);

        // 3. map each to DecryptedSecret
        //    return DecryptedSecret with Value = null, only decrypt on Get
        return entries.Select(e => new DecryptedSecret
        {
            Id = e.Id,
            Name = e.Name,
            Namespace = e.Namespace,
            Value = null, // don't include value in list
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            ExpiresAt = e.ExpiresAt,
            Notes = e.Notes
        }).ToList();
    }

    /// <summary>
    /// Removes a secret from the vault by its name and namespace.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <exception cref="SecretNotFoundException"></exception>
    public void RemoveSecret(string name, string ns = "default")
    {
        // 1. get vault
        Vault vault = _vaultService.GetVaultOrThrow();

        // 2. find entry → throw if missing
        SecretEntry entry = vault.Secrets.FirstOrDefault(s => s.Name == name && s.Namespace == ns)
            ?? throw new SecretNotFoundException();

        // 3. remove from vault
        vault.Secrets.Remove(entry);

        // 4. save
        _store.SaveVault(vault, _vaultService.GetKeyOrThrow());
    }

    /// <summary>
    /// Updates the value of an existing secret. 
    /// This method generates a new DEK for the updated value, encrypts it, and wraps the new DEK with the KEK. 
    /// It then updates the existing secret entry with the new encrypted value and wrapped DEK, and saves the vault. 
    /// If the secret is not found, it throws a SecretNotFoundException.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="newValue">The new value for the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <exception cref="SecretNotFoundException"></exception>
    public void UpdateSecret(string name, string newValue, string ns = "default")
    {
        // 1. find existing entry
        Vault vault = _vaultService.GetVaultOrThrow();
        SecretEntry entry = vault.Secrets.FirstOrDefault(s => s.Name == name && s.Namespace == ns)
            ?? throw new SecretNotFoundException();

        // 2. generate a NEW DEK (don't reuse the old one)
        byte[] dek = _keyDerivation.GenerateDek();

        // 3. encrypt new value with new DEK
        EncryptionResult payload = _encryptor.EncryptValue(Encoding.UTF8.GetBytes(newValue), dek);

        // 4. wrap new DEK with KEK
        EncryptionResult wrappedDek = _encryptor.WrapKey(dek, _vaultService.GetKeyOrThrow());
        CryptographicOperations.ZeroMemory(dek); // zero the raw DEK immediately

        // 5. update the entry's encrypted fields + UpdatedAt
        entry.Id = entry.Id; // unchanged
        entry.Name = entry.Name; // unchanged
        entry.Namespace = ns; // unchanged
        entry.CreatedAt = entry.CreatedAt; // unchanged
        entry.UpdatedAt = DateTime.UtcNow;
        entry.ExpiresAt = entry.ExpiresAt; // unchanged
        entry.Notes = entry.Notes; // unchanged
        entry.EncryptedValue = payload.Ciphertext;
        entry.Nonce = payload.Nonce;
        entry.Tag = payload.Tag;
        entry.WrappedDek = wrappedDek.Ciphertext;
        entry.DekNonce = wrappedDek.Nonce;
        entry.DekTag = wrappedDek.Tag;

        // 6. save
        _store.SaveVault(vault, _vaultService.GetKeyOrThrow());
    }
}
