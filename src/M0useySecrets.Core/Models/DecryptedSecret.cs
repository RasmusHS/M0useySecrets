namespace M0useySecrets.Core.Models;

public class DecryptedSecret
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

    /// <summary>
    /// The actual secret.
    /// Stored in plaintext inside the vault, but entire vault is encrypted at rest.
    /// </summary>
    public string Value { get; set; }

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

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}
