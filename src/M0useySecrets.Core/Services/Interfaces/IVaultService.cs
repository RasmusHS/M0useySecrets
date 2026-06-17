using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Services.Interfaces;

/// <summary>
/// The VaultService is responsible for managing the lifecycle of the vault, including initialization, unlocking, locking, and changing the master password.
/// </summary>
public interface IVaultService
{
    /// <summary>
    /// Initializes a new vault with the given master password. 
    /// This should only be called once, and will throw if a vault already exists. 
    /// It generates a new salt, derives the KEK, creates an empty vault, and saves it to storage. 
    /// After this, the vault is unlocked and ready for use.
    /// </summary>
    /// <param name="masterPassword">The master password to initialize the vault with.</param>
    /// <exception cref="InvalidOperationException">Thrown if the vault is already initialized.</exception>
    void Initialize(string masterPassword);

    /// <summary>
    /// Unlocks the vault with the given master password.
    /// </summary>
    /// <param name="masterPassword">The master password to unlock the vault with.</param>
    /// <exception cref="InvalidOperationException">Thrown if the vault cannot be unlocked with the provided password.</exception>
    void Unlock(string masterPassword);

    /// <summary>
    /// Locks the vault by disposing the key context and setting both the key context and vault references to null.
    /// </summary>
    void Lock();

    /// <summary>
    /// Changes the master password by re-deriving the KEK with the new password and a new salt, 
    /// then re-wrapping all secrets' DEKs with the new KEK, and finally saving the updated vault.
    /// </summary>
    /// <param name="currentPassword">The current master password.</param>
    /// <param name="newPassword">The new master password.</param>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the current password is incorrect.</exception>
    void ChangePassword(string currentPassword, string newPassword);

    /// <summary>
    /// Gets the currently loaded vault, or throws if the vault is locked.
    /// </summary>
    /// <returns>The currently loaded vault.</returns>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    Vault GetVaultOrThrow();

    /// <summary>
    /// Gets the current KEK from the key context, or throws if the vault is locked.
    /// </summary>
    /// <returns>The current KEK.</returns>
    /// <exception cref="VaultLockedException">Thrown if the vault is locked.</exception>
    byte[] GetKeyOrThrow();
}
