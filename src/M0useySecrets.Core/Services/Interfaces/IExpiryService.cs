using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Services.Interfaces;

/// <summary>
/// Service responsible for handling expiration of secrets. 
/// It provides functionality to identify expired secrets and purge them from the vault. 
/// The service interacts with the vault to retrieve secrets, check their expiration status, and remove expired secrets while ensuring that the vault is saved after modifications.
/// </summary>
public interface IExpiryService
{
    /// <summary>
    /// Returns a list of secrets that are expired based on their ExpiresAt property.
    /// </summary>
    /// <param name="secrets">The list of secrets to check for expiration.</param>
    /// <returns>A list of expired secrets as DecryptedSecret objects.</returns>
    List<DecryptedSecret> GetExpiredSecrets(List<SecretEntry> secrets);

    /// <summary>
    /// Removes expired secrets from the vault and saves the updated vault. Returns the count of removed secrets.
    /// </summary>
    /// <returns>The number of expired secrets that were removed.</returns>
    int PurgeExpired();
}
