using System.Net;
using NetworkCore;
using NetworkCore.Events;
using NetworkCore.Protocol;

namespace SharpView.Server;

/// <summary>
/// Bidirectional chat window for the Host side.
/// Sends/receives <see cref="ChatPacket"/> via TCP and typing indicators via UDP.
/// </summary>
public partial class ChatWindow : Form
{
    private readonly AsyncTcpClient _client;
    private readonly string _senderName;

    // ─── UDP Typing Indicator ───
    private readonly AsyncUdpMessenger? _udpMessenger;
    private readonly IPEndPoint? _peerUdpEndpoint;

    /// <summary>Debounce interval: send "Stopped" after this many ms of no typing.</summary>
    private const int TypingSendDebounceMs = 2000;
    /// <summary>Receive timeout: hide indicator after this many ms of no UDP signal.</summary>
    private const int TypingReceiveTimeoutMs = 3000;

    private readonly System.Windows.Forms.Timer _typingSendTimer;
    private readonly System.Windows.Forms.Timer _typingReceiveTimer;
    private bool _isLocalTyping;

    public ChatWindow(AsyncTcpClient client, string senderName,
        AsyncUdpMessenger? udpMessenger = null, IPEndPoint? peerUdpEndpoint = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _senderName = string.IsNullOrWhiteSpace(senderName)
            ? ChatStrings.DefaultSenderName : senderName;
        _udpMessenger = udpMessenger;
        _peerUdpEndpoint = peerUdpEndpoint;

        InitializeComponent();

        // Wire TCP chat events
        btnSend.Click += BtnSend_Click;
        txtMessage.KeyDown += TxtMessage_KeyDown;
        this.FormClosing += ChatWindow_FormClosing;

        // Wire typing indicator (TextChanged for debounce)
        txtMessage.TextChanged += TxtMessage_TextChanged;

        // Send-side debounce timer (fires on UI thread)
        _typingSendTimer = new System.Windows.Forms.Timer { Interval = TypingSendDebounceMs };
        _typingSendTimer.Tick += TypingSendTimer_Tick;

        // Receive-side timeout timer (fires on UI thread)
        _typingReceiveTimer = new System.Windows.Forms.Timer { Interval = TypingReceiveTimeoutMs };
        _typingReceiveTimer.Tick += TypingReceiveTimer_Tick;

        // Subscribe to UDP receive events
        if (_udpMessenger is not null)
            _udpMessenger.DataReceived += UdpMessenger_DataReceived;

        AppendSystemMessage(ChatStrings.ChatStarted);
    }

    // ═══════════════════════ Public API ═══════════════════════

    public void AppendIncomingMessage(ChatPacket chatPacket)
    {
        SafeInvoke(() =>
        {
            var localTime = chatPacket.Timestamp.ToLocalTime();
            AppendColoredLine($"[{localTime:HH:mm:ss}] {chatPacket.SenderName}:", Color.FromArgb(100, 180, 255));
            AppendColoredLine($"  {chatPacket.Message}", Color.FromArgb(210, 215, 225));
            AppendEmptyLine();
            ScrollToEnd();

            // Hide typing indicator when an actual message arrives
            lblTypingIndicator.Visible = false;
            _typingReceiveTimer.Stop();
        });
    }

    public void AppendSystemMessage(string message)
    {
        SafeInvoke(() =>
        {
            AppendColoredLine($"[{DateTime.Now:HH:mm:ss}] [{ChatStrings.SystemSenderName}] {message}",
                Color.FromArgb(180, 180, 190));
            AppendEmptyLine();
            ScrollToEnd();
        });
    }

    public void AppendErrorMessage(string message)
    {
        SafeInvoke(() =>
        {
            AppendColoredLine($"[{DateTime.Now:HH:mm:ss}] {message}", Color.FromArgb(255, 100, 100));
            AppendEmptyLine();
            ScrollToEnd();
        });
    }

    // ═══════════════════════ TCP Chat Events ═══════════════════════

    private async void BtnSend_Click(object? sender, EventArgs e)
        => await SendCurrentMessageAsync();

    private async void TxtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            await SendCurrentMessageAsync();
        }
    }

    private void ChatWindow_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
        }
    }

    // ═══════════════════════ Typing Indicator — SEND side ═══════════════════════

    private void TxtMessage_TextChanged(object? sender, EventArgs e)
    {
        if (_udpMessenger is null || _peerUdpEndpoint is null) return;

        if (string.IsNullOrEmpty(txtMessage.Text))
        {
            if (_isLocalTyping)
            {
                _isLocalTyping = false;
                _typingSendTimer.Stop();
                _ = SendTypingStatusAsync(TypingStatus.Stopped);
            }
            return;
        }

        if (!_isLocalTyping)
        {
            _isLocalTyping = true;
            _ = SendTypingStatusAsync(TypingStatus.Typing);
        }

        _typingSendTimer.Stop();
        _typingSendTimer.Start();
    }

    private async void TypingSendTimer_Tick(object? sender, EventArgs e)
    {
        _typingSendTimer.Stop();
        _isLocalTyping = false;
        await SendTypingStatusAsync(TypingStatus.Stopped);
    }

    private async Task SendTypingStatusAsync(TypingStatus status)
    {
        if (_udpMessenger is null || _peerUdpEndpoint is null) return;
        try
        {
            await _udpMessenger.SendSignalAsync(
                _peerUdpEndpoint.Address,
                _peerUdpEndpoint.Port,
                new[] { (byte)status });
        }
        catch { /* UDP is fire-and-forget */ }
    }

    // ═══════════════════════ Typing Indicator — RECEIVE side ═══════════════════════

    private void UdpMessenger_DataReceived(object? sender, UdpDataReceivedEventArgs e)
    {
        if (e.Data.Length < 1) return;
        var status = (TypingStatus)e.Data[0];

        SafeInvoke(() =>
        {
            if (status == TypingStatus.Typing)
            {
                lblTypingIndicator.Text = ChatStrings.PartnerTypingText;
                lblTypingIndicator.Visible = true;
                _typingReceiveTimer.Stop();
                _typingReceiveTimer.Start();
            }
            else
            {
                lblTypingIndicator.Visible = false;
                _typingReceiveTimer.Stop();
            }
        });
    }

    private void TypingReceiveTimer_Tick(object? sender, EventArgs e)
    {
        _typingReceiveTimer.Stop();
        lblTypingIndicator.Visible = false;
    }

    // ═══════════════════════ TCP Send Logic ═══════════════════════

    private async Task SendCurrentMessageAsync()
    {
        var message = txtMessage.Text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        txtMessage.Clear();
        txtMessage.Focus();

        var chatPacket = new ChatPacket
        {
            SenderName = _senderName,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var localTime = chatPacket.Timestamp.ToLocalTime();
            AppendColoredLine($"[{localTime:HH:mm:ss}] {_senderName}:", Color.FromArgb(120, 220, 160));
            AppendColoredLine($"  {message}", Color.FromArgb(210, 215, 225));
            AppendEmptyLine();
            ScrollToEnd();

            await _client.SendDataAsync(chatPacket.BuildPacket());
        }
        catch (Exception)
        {
            AppendErrorMessage(ChatStrings.SendErrorMessage);
        }
    }

    // ═══════════════════════ Helpers ═══════════════════════

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }

    private void AppendColoredLine(string text, Color color)
    {
        rtbChatHistory.SelectionStart = rtbChatHistory.TextLength;
        rtbChatHistory.SelectionLength = 0;
        rtbChatHistory.SelectionColor = color;
        rtbChatHistory.AppendText(text + Environment.NewLine);
    }

    private void AppendEmptyLine() => rtbChatHistory.AppendText(Environment.NewLine);

    private void ScrollToEnd()
    {
        rtbChatHistory.SelectionStart = rtbChatHistory.TextLength;
        rtbChatHistory.ScrollToCaret();
    }
}
