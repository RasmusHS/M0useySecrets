namespace M0useySecrets.Core.Models;

/// <summary>
/// In-memory working state, never written to disk directly
/// </summary>
public class Vault
{
    /// <summary>
    /// The salt used for deriving the KEK from the master password.
    /// </summary>
    public byte[] Salt { get; set; } // needed to re-derive KEK on password change

    /// <summary>
    /// The number of iterations used in the key derivation function.
    /// </summary>
    public int Iterations { get; set; }

    /// <summary>
    /// The list of secrets stored in the vault. 
    /// Each secret entry contains the encrypted DEK and the encrypted secret value, along with metadata.
    /// </summary>
    public List<SecretEntry> Secrets { get; set; }
}
