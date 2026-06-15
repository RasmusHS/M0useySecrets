namespace M0useySecrets.Core.Models;

/// <summary>
/// Represents the structure of the vault file stored on disk. 
/// This class is used for serialization and deserialization of the vault file content.
/// </summary>
public class VaultFile
{
    public int FormatVersion { get; set; }
    public byte[] Salt { get; set; } // base64 automatic via STJ
    public int Iterations { get; set; }
    public byte[] PasswordCheck { get; set; }
    public byte[] PasswordCheckNonce { get; set; }
    public byte[] PasswordCheckTag { get; set; }
    public byte[] EncryptedPayload { get; set; } // The outer AES-GCM ciphertext | List<SecretEntry> as JSON, encrypted with KEK
    public byte[] PayloadNonce { get; set; }
    public byte[] PayloadTag { get; set; }
}
