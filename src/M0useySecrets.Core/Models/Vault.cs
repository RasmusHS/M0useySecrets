namespace M0useySecrets.Core.Models;

/// <summary>
/// In-memory working state, never written to disk directly
/// </summary>
public class Vault
{
    //public VaultMetadata Metadata { get; set; }
    public byte[] Salt { get; set; } // needed to re-derive KEK on password change

    public int Iterations { get; set; }

    public List<SecretEntry> Secrets { get; set; }
}
