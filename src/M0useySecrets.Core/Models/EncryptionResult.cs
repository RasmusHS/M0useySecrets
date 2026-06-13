namespace M0useySecrets.Core.Models;

public record EncryptionResult(byte[] Ciphertext, byte[] Nonce, byte[] Tag);
