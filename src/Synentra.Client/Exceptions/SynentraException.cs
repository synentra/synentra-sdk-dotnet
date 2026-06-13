namespace Synentra.Client.Exceptions;

/// <summary>
/// Base exception type for all errors originating from the Synentra SDK.
/// </summary>
public class SynentraException : Exception
{
    /// <summary>Initializes a new instance with the specified message.</summary>
    public SynentraException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified message and inner exception.</summary>
    public SynentraException(string message, Exception innerException) : base(message, innerException) { }
}
