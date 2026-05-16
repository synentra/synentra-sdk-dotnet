namespace Vectra.Client.Exceptions;

/// <summary>
/// Base exception type for all errors originating from the Vectra SDK.
/// </summary>
public class VectraException : Exception
{
    /// <summary>Initializes a new instance with the specified message.</summary>
    public VectraException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified message and inner exception.</summary>
    public VectraException(string message, Exception innerException) : base(message, innerException) { }
}
