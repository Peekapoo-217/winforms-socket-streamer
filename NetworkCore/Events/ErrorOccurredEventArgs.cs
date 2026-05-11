namespace NetworkCore.Events;

/// <summary>
/// Event data raised when an internal error occurs during network operations.
/// </summary>
public sealed class ErrorOccurredEventArgs : EventArgs
{
    /// <summary>
    /// A human-readable context message describing where the error occurred.
    /// </summary>
    public string Context { get; }

    /// <summary>
    /// The exception that was caught.
    /// </summary>
    public Exception Exception { get; }

    public ErrorOccurredEventArgs(string context, Exception exception)
    {
        Context = context;
        Exception = exception;
    }
}
