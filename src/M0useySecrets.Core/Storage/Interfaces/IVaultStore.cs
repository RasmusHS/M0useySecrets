using M0useySecrets.Core.Models;

namespace M0useySecrets.Core.Storage.Interfaces;

public interface IVaultStore
{
    /// <summary>
    /// Creates a new vault file with the given KEK. The vault will be initialized with an empty secrets list.
    /// </summary>
    /// <param name="vault">The vault object to initialize.</param>
    /// <param name="kek">The key encryption key (KEK) used to encrypt the vault.</param>
    void CreateVault(Vault vault, byte[] kek);

    /// <summary>
    /// Saves the given vault to disk, encrypting it with the provided KEK. 
    /// This method performs an atomic write by first writing to a temporary file and then renaming it.
    /// </summary>
    /// <param name="vault">The vault object to save.</param>
    /// <param name="kek">The key encryption key (KEK) used to encrypt the vault.</param>
    void SaveVault(Vault vault, byte[] kek);

    /// <summary>
    /// Loads just the vault header (salt and iterations) from disk without attempting to decrypt the secrets.
    /// </summary>
    /// <returns>The vault header containing the salt and iteration count.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the vault file is corrupt or empty.</exception>
    VaultHeader LoadHeader();

    /// <summary>
    /// Loads the entire vault from disk, decrypting it with the provided KEK.
    /// </summary>
    /// <param name="kek">The key encryption key (KEK) used to decrypt the vault.</param>
    /// <returns>The decrypted vault containing the secrets and metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the vault file is corrupt or empty.</exception>
    /// <exception cref="InvalidPasswordException">Thrown if the provided KEK is invalid.</exception>
    Vault LoadVault(byte[] kek);
}
