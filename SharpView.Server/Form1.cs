using NetworkCore;
using NetworkCore.Events;
using NetworkCore.Protocol;
using SharpView.Server.Helpers;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace SharpView.Server;

public partial class Form1 : Form
{
    private AsyncTcpClient? _client;
    private ScreenCapturer? _capturer;
    private CancellationTokenSource? _streamCts;
    private bool _isStreaming;
    private int _streamIntervalMs = 80;
    private const int JpegQuality = 50;

    // ─── Partner ID + Password ───
    private readonly string _partnerId;
    private string _password = "";
    private bool _isPaired = false;

    public Form1()
    {
        InitializeComponent();
        this.FormClosing += Form1_FormClosing;

        // Generate the persistent Partner ID from hardware UUID
        _partnerId = GeneratePartnerId();
        lblPartnerId.Text = FormatPartnerId(_partnerId);

        // Generate a fresh 4-digit password
        RegeneratePassword();
    }

    // ═══════════════════════ Partner ID & Password Generation ═══════════════════════

    /// <summary>
    /// Extracts the Hardware UUID via WMI (Win32_ComputerSystemProduct)
    /// and hashes it into a deterministic 9-digit Partner ID.
    /// </summary>
    private static string GeneratePartnerId()
    {
        string uuid = "UNKNOWN";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (var obj in searcher.Get())
            {
                uuid = obj["UUID"]?.ToString() ?? "UNKNOWN";
                break;
            }
        }
        catch { /* Fallback to "UNKNOWN" */ }

        // Deterministic hash → 9-digit positive integer
        int hash = Math.Abs(uuid.GetHashCode());
        string raw = hash.ToString().PadLeft(9, '0');

        // Ensure exactly 9 digits (truncate if longer)
        return raw.Length > 9 ? raw[..9] : raw;
    }

    /// <summary>Formats "123456789" → "123 456 789" for display.</summary>
    private static string FormatPartnerId(string id)
    {
        if (id.Length != 9) return id;
        return $"{id[..3]} {id[3..6]} {id[6..9]}";
    }

    /// <summary>Generates a new random 4-digit password and updates the UI label.</summary>
    private void RegeneratePassword()
    {
        _password = Random.Shared.Next(1000, 10000).ToString();
        lblPassword.Text = _password;
    }

    // ═══════════════════════ Host Start / Stop ═══════════════════════

    private async void BtnStart_Click(object? sender, EventArgs e)
    {
        var ip = txtRelayIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        {
            MessageBox.Show("Enter the Relay server IP.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtRelayIp.Focus(); return;
        }

        if (!int.TryParse(txtPort.Text.Trim(), out var port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Enter a valid port (1–65535).", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPort.Focus(); return;
        }

        try
        {
            _isPaired = false;
            RegeneratePassword(); // Fresh password each time

            _client = new AsyncTcpClient(ip, port);
            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;
            _client.DataReceived += Client_DataReceived;
            _client.ErrorOccurred += Client_ErrorOccurred;

            AppendLog($"Connecting to Relay at {ip}:{port}...", Color.Gray);
            await _client.ConnectAsync();
            SetServerState(true);

            // Register Host with Partner ID + Password
            var packet = SessionPacket.BuildRegisterHost(_partnerId, _password);
            await _client.SendDataAsync(packet);

            AppendLog($"Registering Partner ID: {FormatPartnerId(_partnerId)} | Password: {_password}", Color.DeepSkyBlue);
        }
        catch (Exception ex) { AppendLog($"Start failed: {ex.Message}", Color.OrangeRed); }
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        StopStreaming(); StopServer();
        AppendLog("Host stopped.", Color.IndianRed);
        SetServerState(false);
    }

    private void BtnToggleStream_Click(object? sender, EventArgs e)
    {
        if (_isStreaming) StopStreaming(); else StartStreaming();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        StopStreaming(); StopServer();
    }

    // ═══════════════════════ Streaming ═══════════════════════
    // (UNTOUCHED — same as before)

    private void StartStreaming()
    {
        if (_client is null || !_isPaired)
        { AppendLog("Cannot stream — not paired with a viewer.", Color.OrangeRed); return; }

        _capturer = new ScreenCapturer(JpegQuality);
        _streamCts = new CancellationTokenSource();
        _isStreaming = true;
        btnToggleStream.Text = "⏹ Stop";
        btnToggleStream.BackColor = Color.FromArgb(178, 34, 34);
        AppendLog($"Streaming started ({_streamIntervalMs}ms, q={JpegQuality}).", Color.DeepSkyBlue);
        _ = StreamLoopAsync(_streamCts.Token);
    }

    private void StopStreaming()
    {
        if (!_isStreaming) return;
        _streamCts?.Cancel(); _streamCts?.Dispose(); _streamCts = null;
        _capturer = null; _isStreaming = false;
        btnToggleStream.Text = "📺 Stream";
        btnToggleStream.BackColor = Color.FromArgb(70, 130, 180);
        lblFps.Text = "";
        AppendLog("Streaming stopped.", Color.Salmon);
    }

    private async Task StreamLoopAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        int frameCount = 0;
        var fpsTimer = Stopwatch.StartNew();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                sw.Restart();
                var imgBytes = _capturer!.CaptureScreen();
                var packet = PacketBuilder.Build(DataType.Image, imgBytes);

                if (_client is not null && _client.IsConnected && _isPaired)
                {
                    try { await _client.SendDataAsync(packet); }
                    catch { /* connection dropped */ }
                }

                frameCount++;
                if (fpsTimer.ElapsedMilliseconds >= 1000)
                {
                    var fps = frameCount; var kb = imgBytes.Length / 1024;
                    frameCount = 0; fpsTimer.Restart();
                    SafeInvoke(() => lblFps.Text = $"{fps} FPS | ~{kb} KB/f");
                }
                var delay = _streamIntervalMs - (int)sw.ElapsedMilliseconds;
                if (delay > 0) await Task.Delay(delay, ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SafeInvoke(() => AppendLog($"Stream error: {ex.Message}", Color.OrangeRed)); }
    }

    // ═══════════════════════ Client Events ═══════════════════════

    private void Client_Connected(object? sender, EventArgs e)
    {
        SafeInvoke(() => AppendLog("Connected to Relay.", Color.LimeGreen));
    }

    private void Client_Disconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        SafeInvoke(() =>
        {
            AppendLog($"Disconnected — {e.Reason ?? "unknown"}", Color.Salmon);
            _isPaired = false;
            StopStreaming();
            SetServerState(false);
        });
    }

    private void Client_DataReceived(object? sender, NetworkCore.Events.DataReceivedEventArgs e)
    {
        var p = PacketParser.Parse(e.Data);

        switch (p.Type)
        {
            case DataType.HostRegistered:
                var regPacket = SessionPacket.FromBytes(p.Payload);
                SafeInvoke(() =>
                {
                    panelInfoBar.Visible = true;
                    AppendLog($"Registered on Relay! Partner ID: {FormatPartnerId(regPacket.PartnerId)}. Waiting for viewer...", Color.DeepSkyBlue);
                });
                break;

            case DataType.SessionPaired:
                _isPaired = true;
                SafeInvoke(() =>
                {
                    AppendLog("[🔓] Viewer joined! Starting stream...", Color.LimeGreen);
                    UpdateStreamButton();
                    StartStreaming(); // Auto-start streaming
                });
                break;

            // ─── Input commands (UNTOUCHED) ───

            case DataType.MouseCommand:
                if (!_isPaired) return;
                try
                {
                    var mouse = MousePacket.FromBytes(p.Payload);
                    InputSimulator.SimulateMouse(mouse);
                }
                catch (Exception ex)
                {
                    SafeInvoke(() => AppendLog($"[⚠] Mouse error: {ex.Message}", Color.OrangeRed));
                }
                break;

            case DataType.KeyboardCommand:
                if (!_isPaired) return;
                try
                {
                    var kb = KeyboardPacket.FromBytes(p.Payload);
                    InputSimulator.SimulateKeyboard(kb);
                }
                catch (Exception ex)
                {
                    SafeInvoke(() => AppendLog($"[⚠] Keyboard error: {ex.Message}", Color.OrangeRed));
                }
                break;

            case DataType.Text:
                if (!_isPaired) return;
                SafeInvoke(() => AppendLog($"[📩] Viewer: {Encoding.UTF8.GetString(p.Payload)}", Color.Gold));
                break;

            default:
                SafeInvoke(() => AppendLog($"[?] Type={p.Type}", Color.Gray));
                break;

            // ─── Dynamic Quality Scaling ───

            case DataType.QualityCommand:
                if (!_isPaired) return;
                var qPacket = QualityPacket.FromBytes(p.Payload);
                _streamIntervalMs = qPacket.StreamIntervalMs;
                _capturer?.UpdateQuality(qPacket.JpegQuality);
                SafeInvoke(() => AppendLog($"[📊] Quality adjusted: JPEG={qPacket.JpegQuality}%, Interval={qPacket.StreamIntervalMs}ms", Color.FromArgb(100, 200, 255)));
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

    private void UpdateStreamButton() => btnToggleStream.Enabled = _isPaired;

    private void SetServerState(bool running)
    {
        btnStart.Enabled = !running; btnStop.Enabled = running;
        txtPort.Enabled = !running; txtRelayIp.Enabled = !running;
        UpdateStreamButton();
        lblStatus.Text = running ? "  ● Connected to Relay" : "  ● Stopped";
        lblStatus.ForeColor = running ? Color.LimeGreen : Color.IndianRed;

        if (!running)
        {
            panelInfoBar.Visible = false;
            _isPaired = false;
        }
    }

    private void StopServer()
    {
        if (_client is null) return;
        _client.Connected -= Client_Connected;
        _client.Disconnected -= Client_Disconnected;
        _client.DataReceived -= Client_DataReceived;
        _client.ErrorOccurred -= Client_ErrorOccurred;
        _client.Dispose(); _client = null;
    }
}
