namespace M0useySecrets.Core.Exceptions;

public class InvalidPasswordException : UnauthorizedAccessException
{
    // parameterless - uses a default message
    public InvalidPasswordException() : base("Wrong password")
    {
    }

    // constructor that accepts a custom message
    public InvalidPasswordException(string message) : base(message)
    {
    }

    // constructor that accepts a custom message and an inner exception
    public InvalidPasswordException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
