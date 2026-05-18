namespace SharpView.Server;

/// <summary>
/// Centralizes all UI-facing string constants for the Chat feature.
/// Avoids hardcoded strings scattered throughout form logic.
/// </summary>
internal static class ChatStrings
{
    // ─── Window ───
    internal const string WindowTitle = "💬 Chat";

    // ─── Buttons / Labels ───
    internal const string SendButtonText = "Gửi";
    internal const string InputPlaceholder = "Nhập tin nhắn...";

    // ─── Chat display ───
    internal const string SystemSenderName = "Hệ thống";
    internal const string SendErrorMessage = "[Lỗi hệ thống] Không thể gửi tin nhắn.";
    internal const string PartnerDisconnected = "Đối tác đã ngắt kết nối.";
    internal const string ChatStarted = "Cuộc trò chuyện đã bắt đầu. Hãy gửi tin nhắn!";

    // ─── Sender labels ───
    internal const string DefaultSenderName = "Host";

    // ─── Typing Indicator ───
    internal const string PartnerTypingText = "Đối tác đang gõ...";

    // ─── Copy Buttons ───
    internal const string CopiedTooltip = "Đã sao chép!";
}
