namespace M0useySecrets.Core.Models;

public class Vault
{
    public VaultMetadata Metadata { get; set; }

    public List<SecretEntry> Secrets { get; set; }
}
