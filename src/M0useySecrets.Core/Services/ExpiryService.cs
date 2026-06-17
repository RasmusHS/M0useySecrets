using System.Security.Cryptography;
using M0useySecrets.Core.Models;
using M0useySecrets.Core.Services.Interfaces;
using M0useySecrets.Core.Storage.Interfaces;

namespace M0useySecrets.Core.Services;

/// <summary>
/// Service responsible for handling expiration of secrets. 
/// It provides functionality to identify expired secrets and purge them from the vault. 
/// The service interacts with the vault to retrieve secrets, check their expiration status, and remove expired secrets while ensuring that the vault is saved after modifications.
/// </summary>
public class ExpiryService : IExpiryService
{
    private readonly IVaultService _vaultService;
    private readonly IVaultStore _store;

    public ExpiryService(IVaultService vaultService, IVaultStore store)
    {
        _vaultService = vaultService;
        _store = store;
    }

    /// <summary>
    /// Returns a list of secrets that are expired based on their ExpiresAt property.
    /// </summary>
    /// <param name="secrets">The list of secrets to check for expiration.</param>
    /// <returns>A list of expired secrets as DecryptedSecret objects.</returns>
    public List<DecryptedSecret> GetExpiredSecrets(List<SecretEntry> secrets)
    {
        // filter where ExpiresAt != null && ExpiresAt < DateTime.UtcNow
        var expiredSecrets = secrets
            .Where(s => s.ExpiresAt.HasValue && s.ExpiresAt.Value < DateTime.UtcNow)
            .Select(s => new DecryptedSecret
            {
                Id = s.Id,
                Name = s.Name,
                Namespace = s.Namespace,
                Value = null, // we won't decrypt values here, just return metadata
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                ExpiresAt = s.ExpiresAt,
                Notes = s.Notes
            })
            .ToList();

        // return as DecryptedSecret (without decrypting values — just metadata)
        return expiredSecrets;
    }

    /// <summary>
    /// Removes expired secrets from the vault and saves the updated vault. Returns the count of removed secrets.
    /// </summary>
    /// <returns>The number of expired secrets that were removed.</returns>
    public int PurgeExpired()
    {
        // 1. get vault (which includes secrets)
        Vault vault = _vaultService.GetVaultOrThrow();

        // 2. find all expired entries
        var expiredSecrets = vault.Secrets
            .Where(s => s.ExpiresAt.HasValue && s.ExpiresAt.Value < DateTime.UtcNow)
            .ToList();

        if (expiredSecrets.Count <= 0)
            return 0;

        // 3. remove each from vault.Secrets
        foreach (var secret in expiredSecrets)
        {
            vault.Secrets.Remove(secret);
        }

        // 4. save once at the end (not per removal)
        byte[] kek = _vaultService.GetKeyOrThrow();
        _store.SaveVault(vault, kek);
        CryptographicOperations.ZeroMemory(kek); // zero out KEK after use

        // 5. return count removed
        return expiredSecrets.Count;
    }
}
