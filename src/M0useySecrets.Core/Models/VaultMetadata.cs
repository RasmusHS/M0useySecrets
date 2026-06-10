namespace M0useySecrets.Core.Models;

public class VaultMetadata
{
    /// <summary>
    /// Random, generated once at vault init (32 bytes)
    /// </summary>
    public byte[] Salt { get; set; }

    /// <summary>
    /// PBKDF2 iteration count (≥600_000 for SHA-256)
    /// </summary>
    public int Iterations { get; set; }

    /// <summary>
    /// Start at 1, bump if you change the file format
    /// </summary>
    public int FormatVersion { get; set; }

    /// <summary>
    /// Encrypt a known sentinel value with the derived key so you can distinguish "wrong password" from "corrupt file"
    /// </summary>
    public byte[] PasswordCheck { get; set; }

    public byte[] PasswordCheckNonce { get; set; }

    public byte[] PasswordCheckTag { get; set; }
}
