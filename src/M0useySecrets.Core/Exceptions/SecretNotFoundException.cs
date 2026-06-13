namespace M0useySecrets.Core.Exceptions;

public class SecretNotFoundException : KeyNotFoundException
{
    // parameterless - uses a default message
    public SecretNotFoundException() : base("Secret not found.")
    {
    }

    // constructor that accepts a custom message
    public SecretNotFoundException(string message) : base(message)
    {
    }

    // constructor that accepts a custom message and an inner exception
    public SecretNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
