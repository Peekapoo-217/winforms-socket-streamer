namespace SharpView.Client;

/// <summary>
/// Event data for Quick Connect requests from the Chat window.
/// Contains the Partner ID and Password extracted by Regex.
/// </summary>
public sealed class QuickConnectEventArgs : EventArgs
{
    /// <summary>9-digit Partner ID extracted from chat text.</summary>
    public string PartnerId { get; }

    /// <summary>4-digit Password extracted from chat text.</summary>
    public string Password { get; }

    public QuickConnectEventArgs(string partnerId, string password)
    {
        PartnerId = partnerId;
        Password = password;
    }
}
