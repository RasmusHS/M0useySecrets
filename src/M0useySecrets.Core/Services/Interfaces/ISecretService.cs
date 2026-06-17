using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Services.Interfaces;

/// <summary>
/// Service responsible for managing secrets within the vault, including adding, retrieving, listing, updating, and removing secrets.
/// </summary>
public interface ISecretService
{
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
    void AddSecret(string name, string value, string ns = "default", DateTime? expiresAt = null, string? notes = null);

    /// <summary>
    /// Retrieves and decrypts a secret by its name and namespace.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <returns>The decrypted secret.</returns>
    /// <exception cref="SecretNotFoundException"></exception>
    DecryptedSecret GetSecret(string name, string ns = "default");

    /// <summary>
    /// Lists all secrets in the vault, optionally filtered by namespace.
    /// </summary>
    /// <param name="ns">The namespace to filter secrets by. If null, all secrets are returned.</param>
    /// <returns>A list of decrypted secrets with their values set to null.</returns>
    List<DecryptedSecret> ListSecrets(string? ns = null);

    /// <summary>
    /// Removes a secret from the vault by its name and namespace.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="ns">The namespace of the secret.</param>
    /// <exception cref="SecretNotFoundException"></exception>
    void RemoveSecret(string name, string ns = "default");

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
    void UpdateSecret(string name, string newValue, string ns = "default");
}
