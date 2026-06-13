namespace M0useySecrets.Core.Models;

public class SecretEntry
{
    /// <summary>
    /// The unique identifier for the vault. 
    /// This is used to reference the vault in various operations, such as retrieving its contents or updating its information.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Lookup key, unique within the namespace. 
    /// This is used to identify the vault in a human-readable way, and can be used for display purposes or as a reference in user interfaces.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Defaults to "default".
    /// Groups related secrets together.
    /// </summary>
    public string Namespace { get; set; } = "default";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Null means never expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Optional freetext
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Secret value encrypted with this entry's DEK
    /// </summary>
    public byte[] EncryptedValue { get; set; }

    /// <summary>
    /// The GCM nonce used for value encryption
    /// </summary>
    public byte[] Nonce { get; set; }

    /// <summary>
    /// The GCM auth tag for value encryption
    /// </summary>
    public byte[] Tag { get; set; }

    /// <summary>
    /// This entry's DEK, encrypted with the KEK
    /// </summary>
    public byte[] WrappedDek { get; set; }

    /// <summary>
    /// Nonce used when wrapping the DEK
    /// </summary>
    public byte[] DekNonce { get; set; }

    /// <summary>
    /// Auth tag from wrapping the DEK
    /// </summary>
    public byte[] DekTag { get; set; }
}
