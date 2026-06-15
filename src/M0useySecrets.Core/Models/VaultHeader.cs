namespace M0useySecrets.Core.Models;

public record VaultHeader(byte[] Salt, int Iterations);
