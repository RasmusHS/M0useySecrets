namespace M0useySecrets.Core.Exceptions;

public class VaultLockedException : InvalidOperationException
{
    // parameterless - uses a default message
    public VaultLockedException() : base("Vault is locked. Unlock with master password before accessing secrets.")
    {
    }

    // constructor that accepts a custom message
    public VaultLockedException(string message) : base(message)
    {
    }

    // constructor that accepts a custom message and an inner exception
    public VaultLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
