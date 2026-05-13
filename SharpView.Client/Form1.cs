using NetworkCore;
using NetworkCore.Events;
using NetworkCore.Protocol;
using SharpView.Client.Helpers;
using System.Diagnostics;
using System.Text;

namespace SharpView.Client;

public partial class Form1 : Form
{
    private AsyncTcpClient? _client;
    private int _frameCount;
    private readonly Stopwatch _fpsTimer = new();

    // ─── Relay Session State ───
    private bool _isPaired;

    // ─── Remote input state ───
    private Size _serverScreenSize = Size.Empty;
    private const int MouseMoveIntervalMs = 33;
    private readonly Stopwatch _mouseMoveTimer = Stopwatch.StartNew();
    private Point _lastSentMousePos = Point.Empty;

    // ─── Dynamic Quality Scaling state ───
    private readonly Stopwatch _frameDeltaTimer = new();
    private readonly Stopwatch _qualityCooldownTimer = Stopwatch.StartNew();
    private int _currentTrackingQuality = 50;
    private int _currentTrackingInterval = 80;

    public Form1()
    {
        InitializeComponent();
        this.FormClosing += Form1_FormClosing;

        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
        this.KeyUp += Form1_KeyUp;

        pbScreen.MouseMove += PbScreen_MouseMove;
        pbScreen.MouseDown += PbScreen_MouseDown;
        pbScreen.MouseUp += PbScreen_MouseUp;
        pbScreen.MouseWheel += PbScreen_MouseWheel;
    }

    // ═══════════════════════ UI Events ═══════════════════════

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        var ip = txtIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        { MessageBox.Show("Enter a valid IP.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtIp.Focus(); return; }

        if (!int.TryParse(txtPort.Text.Trim(), out var port) || port is < 1 or > 65535)
        { MessageBox.Show("Enter a valid port (1–65535).", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPort.Focus(); return; }

        // Validate Partner ID (9 digits, may contain spaces for display)
        var partnerId = txtPartnerId.Text.Replace(" ", "").Trim();
        if (partnerId.Length != 9 || !long.TryParse(partnerId, out _))
        { MessageBox.Show("Enter a valid 9-digit Partner ID.", "Invalid Partner ID", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPartnerId.Focus(); return; }

        // Validate Password (4 digits)
        var password = txtPassword.Text.Trim();
        if (password.Length != 4 || !int.TryParse(password, out _))
        { MessageBox.Show("Enter a valid 4-digit Password.", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPassword.Focus(); return; }

        try
        {
            _isPaired = false;
            _client = new AsyncTcpClient(ip, port);
            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;
            _client.DataReceived += Client_DataReceived;
            _client.ErrorOccurred += Client_ErrorOccurred;

            AppendLog($"Connecting to Relay at {ip}:{port}...", Color.Gray);
            await _client.ConnectAsync();

            AppendLog($"Joining Partner ID: {partnerId}...", Color.Gold);
            var joinPacket = SessionPacket.BuildJoinSession(partnerId, password);
            await _client.SendDataAsync(joinPacket);
        }
        catch (Exception ex)
        { AppendLog($"Connection failed: {ex.Message}", Color.OrangeRed); CleanupClient(); }
    }

    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        CleanupClient();
        AppendLog("Disconnected by user.", Color.IndianRed);
        SetConnectedState(false);
    }

    private async void BtnPing_Click(object? sender, EventArgs e)
    {
        if (_client is null || !_client.IsConnected || !_isPaired)
        { AppendLog("Not paired.", Color.OrangeRed); return; }

        try
        {
            var packet = PacketBuilder.BuildText($"Ping at {DateTime.Now:HH:mm:ss.fff}");
            await _client.SendDataAsync(packet);
            AppendLog("[📤] Ping sent.", Color.MediumSpringGreen);
        }
        catch (Exception ex)
        { AppendLog($"Send failed: {ex.Message}", Color.OrangeRed); }
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        CleanupClient();
        pbScreen.Image?.Dispose();
    }

    // ═══════════════════════ Remote Mouse Input (UNTOUCHED) ═══════════════════════

    private async void PbScreen_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!CanSendInput()) return;
        if (_mouseMoveTimer.ElapsedMilliseconds < MouseMoveIntervalMs) return;

        var pt = MouseScaler.GetImageCoordinate(pbScreen, e.Location, _serverScreenSize);
        if (pt.X == -1) return;
        if (pt == _lastSentMousePos) return;
        _lastSentMousePos = pt;
        _mouseMoveTimer.Restart();

        await SendMouseAsync(pt.X, pt.Y, "Move");
    }

    private async void PbScreen_MouseDown(object? sender, MouseEventArgs e)
    {
        if (!CanSendInput()) return;
        var pt = MouseScaler.GetImageCoordinate(pbScreen, e.Location, _serverScreenSize);
        if (pt.X == -1) return;

        var action = e.Button switch
        {
            MouseButtons.Left => "LeftDown",
            MouseButtons.Right => "RightDown",
            MouseButtons.Middle => "MiddleDown",
            _ => null
        };
        if (action is not null) await SendMouseAsync(pt.X, pt.Y, action);
    }

    private async void PbScreen_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!CanSendInput()) return;
        var pt = MouseScaler.GetImageCoordinate(pbScreen, e.Location, _serverScreenSize);
        if (pt.X == -1) return;

        var action = e.Button switch
        {
            MouseButtons.Left => "LeftUp",
            MouseButtons.Right => "RightUp",
            MouseButtons.Middle => "MiddleUp",
            _ => null
        };
        if (action is not null) await SendMouseAsync(pt.X, pt.Y, action);
    }

    private async void PbScreen_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (!CanSendInput()) return;
        var pt = MouseScaler.GetImageCoordinate(pbScreen, e.Location, _serverScreenSize);
        if (pt.X == -1) return;
        await SendMouseAsync(pt.X, pt.Y, "Scroll", e.Delta);
    }

    // ═══════════════════════ Remote Keyboard Input (UNTOUCHED) ═══════════════════════

    private async void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!CanSendInput()) return;
        await SendKeyAsync(e.KeyCode.ToString(), isDown: true);
        e.Handled = true;
    }

    private async void Form1_KeyUp(object? sender, KeyEventArgs e)
    {
        if (!CanSendInput()) return;
        await SendKeyAsync(e.KeyCode.ToString(), isDown: false);
        e.Handled = true;
    }

    // ═══════════════════════ Send Helpers (UNTOUCHED) ═══════════════════════

    private bool CanSendInput()
        => _client is not null && _client.IsConnected && _isPaired && _serverScreenSize != Size.Empty;

    private async Task SendMouseAsync(int x, int y, string action, int scrollDelta = 0)
    {
        try
        {
            var packet = new MousePacket { X = x, Y = y, Action = action, ScrollDelta = scrollDelta };
            await _client!.SendDataAsync(packet.BuildPacket());
        }
        catch { }
    }

    private async Task SendKeyAsync(string keyCode, bool isDown)
    {
        try
        {
            var packet = new KeyboardPacket { KeyCode = keyCode, IsDown = isDown };
            await _client!.SendDataAsync(packet.BuildPacket());
        }
        catch { }
    }

    // ═══════════════════════ Client Events ═══════════════════════

    private void Client_Connected(object? sender, EventArgs e)
    {
        SafeInvoke(() =>
        {
            AppendLog("Connected to Relay — verifying credentials...", Color.Gold);
            btnConnect.Enabled = false;
            _frameCount = 0;
            _fpsTimer.Restart();
        });
    }

    private void Client_Disconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        SafeInvoke(() =>
        {
            AppendLog($"Disconnected — {e.Reason ?? "unknown"}", Color.Salmon);
            SetConnectedState(false);
            _fpsTimer.Stop();
            _serverScreenSize = Size.Empty;
            _isPaired = false;
        });
    }

    private void Client_DataReceived(object? sender, NetworkCore.Events.DataReceivedEventArgs e)
    {
        var parsed = PacketParser.Parse(e.Data);

        switch (parsed.Type)
        {
            case DataType.SessionPaired:
                SafeInvoke(() =>
                {
                    _isPaired = true;
                    AppendLog("[🔓] Session paired! Awaiting stream...", Color.LimeGreen);
                    SetConnectedState(true);
                });
                break;

            case DataType.SessionError:
                SafeInvoke(() =>
                {
                    _isPaired = false;
                    var err = SessionPacket.FromBytes(parsed.Payload);
                    AppendLog($"[🔒] {err.Message}", Color.OrangeRed);
                    MessageBox.Show(err.Message, "Connection Rejected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetConnectedState(false);
                });
                break;

            // ─── Streaming (UNTOUCHED) ───

            case DataType.Image:
                if (!_isPaired) return;

                // ─── Dynamic Quality Scaling: measure inter-frame latency ───
                long frameTimeMs = _frameDeltaTimer.ElapsedMilliseconds;
                _frameDeltaTimer.Restart();

                if (_qualityCooldownTimer.ElapsedMilliseconds > 2000)
                {
                    bool needsAdjustment = false;
                    if (frameTimeMs > 180)
                    {
                        _currentTrackingQuality = Math.Max(20, _currentTrackingQuality - 10);
                        _currentTrackingInterval = Math.Min(200, _currentTrackingInterval + 20);
                        needsAdjustment = true;
                    }
                    else if (frameTimeMs < 60 && _currentTrackingQuality < 80)
                    {
                        _currentTrackingQuality = Math.Min(80, _currentTrackingQuality + 5);
                        _currentTrackingInterval = Math.Max(40, _currentTrackingInterval - 10);
                        needsAdjustment = true;
                    }

                    if (needsAdjustment && _client is not null && _client.IsConnected)
                    {
                        var qCmd = new QualityPacket { JpegQuality = _currentTrackingQuality, StreamIntervalMs = _currentTrackingInterval };
                        var packet = PacketBuilder.Build(DataType.QualityCommand, qCmd.ToBytes());
                        _ = _client.SendDataAsync(packet);
                        _qualityCooldownTimer.Restart();
                    }
                }

                // ─── Render frame ───
                SafeInvoke(() =>
                {
                    ImageHelper.UpdatePictureBox(pbScreen, parsed.Payload);

                    if (_serverScreenSize == Size.Empty && pbScreen.Image is not null)
                    {
                        _serverScreenSize = pbScreen.Image.Size;
                        AppendLog($"Server screen: {_serverScreenSize.Width}×{_serverScreenSize.Height}", Color.DeepSkyBlue);
                    }

                    _frameCount++;
                    if (_fpsTimer.ElapsedMilliseconds >= 1000)
                    {
                        lblFrameInfo.Text = $"{_frameCount} FPS  |  ~{parsed.Payload.Length / 1024} KB/f";
                        _frameCount = 0;
                        _fpsTimer.Restart();
                    }
                });
                break;

            case DataType.Text:
                if (!_isPaired) return;
                SafeInvoke(() =>
                {
                    var text = Encoding.UTF8.GetString(parsed.Payload);
                    AppendLog($"[📩] Host: {text}", Color.Gold);
                });
                break;

            default:
                SafeInvoke(() => AppendLog($"[?] Type={parsed.Type} ({parsed.Payload.Length}B)", Color.Gray));
                break;
        }
    }

    private void Client_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        SafeInvoke(() => AppendLog($"[⚠] {e.Context}: {e.Exception.Message}", Color.OrangeRed));
    }

    // ═══════════════════════ Helpers ═══════════════════════

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }

    private void AppendLog(string msg, Color color)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}";
        rtbLogs.SelectionStart = rtbLogs.TextLength;
        rtbLogs.SelectionLength = 0;
        rtbLogs.SelectionColor = color;
        rtbLogs.AppendText(line);
        rtbLogs.ScrollToCaret();
    }

    private void SetConnectedState(bool connected)
    {
        btnConnect.Enabled = !connected; btnDisconnect.Enabled = connected;
        btnPing.Enabled = connected && _isPaired;
        txtIp.Enabled = !connected; txtPort.Enabled = !connected;
        txtPartnerId.Enabled = !connected; txtPassword.Enabled = !connected;
        lblStatus.Text = connected && _isPaired
            ? "  ● Paired"
            : connected ? "  ● Awaiting session..." : "  ● Disconnected";
        lblStatus.ForeColor = connected && _isPaired
            ? Color.LimeGreen
            : connected ? Color.Gold : Color.IndianRed;
        if (!connected) lblFrameInfo.Text = "";
    }

    private void CleanupClient()
    {
        if (_client is null) return;
        _client.Connected -= Client_Connected;
        _client.Disconnected -= Client_Disconnected;
        _client.DataReceived -= Client_DataReceived;
        _client.ErrorOccurred -= Client_ErrorOccurred;
        _client.Dispose(); _client = null;
        _isPaired = false;
    }
}
