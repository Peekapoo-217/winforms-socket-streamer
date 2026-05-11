using System.Drawing;
using System.Runtime.InteropServices;
using NetworkCore.Protocol;

namespace SharpView.Server.Helpers;

/// <summary>
/// Simulates mouse and keyboard input on the server machine using Win32 API.
/// Translates <see cref="MousePacket"/> and <see cref="KeyboardPacket"/>
/// into native input events via <c>user32.dll</c>.
/// </summary>
public static class InputSimulator
{
    // ═══════════════════════════ Win32 Imports ═══════════════════════════

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void mouse_event(
        uint dwFlags, int dx, int dy, int dwData, nint dwExtraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(
        byte bVk, byte bScan, uint dwFlags, nint dwExtraInfo);

    // ─── mouse_event flags ───
    private const uint MOUSEEVENTF_LEFTDOWN   = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP     = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN  = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP    = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP   = 0x0040;
    private const uint MOUSEEVENTF_WHEEL      = 0x0800;

    // ─── keybd_event flags ───
    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP   = 0x0002;

    // ═══════════════════════════ Public API ═══════════════════════════

    /// <summary>
    /// Simulates a mouse event based on the received <see cref="MousePacket"/>.
    /// The cursor is first moved to the absolute (X, Y) position,
    /// then the specified action is performed.
    /// </summary>
    /// <param name="packet">The deserialized mouse command from the client.</param>
    public static void SimulateMouse(MousePacket packet)
    {
        // Always move cursor to the target position first.
        SetCursorPos(packet.X, packet.Y);

        switch (packet.Action)
        {
            case "Move":
                // Cursor already moved above — nothing else to do.
                break;

            case "LeftDown":
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                break;

            case "LeftUp":
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                break;

            case "LeftClick":
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                break;

            case "RightDown":
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                break;

            case "RightUp":
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                break;

            case "RightClick":
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                break;

            case "MiddleDown":
                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                break;

            case "MiddleUp":
                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                break;

            case "Scroll":
                // dwData: positive = scroll up, negative = scroll down.
                // Standard wheel delta unit = 120.
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, packet.ScrollDelta, 0);
                break;
        }
    }

    /// <summary>
    /// Simulates a keyboard event based on the received <see cref="KeyboardPacket"/>.
    /// </summary>
    /// <param name="packet">The deserialized keyboard command from the client.</param>
    /// <returns><c>true</c> if the key code was valid and the event was sent.</returns>
    public static bool SimulateKeyboard(KeyboardPacket packet)
    {
        // Parse the Keys enum name back into its byte virtual-key code.
        if (!Enum.TryParse<ConsoleKey>(packet.KeyCode, ignoreCase: true, out var consoleKey))
        {
            // Fallback: try parsing as a raw virtual key code integer.
            if (!byte.TryParse(packet.KeyCode, out var rawVk))
                return false;

            keybd_event(rawVk, 0, packet.IsDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP, 0);
            return true;
        }

        var vk = (byte)consoleKey;
        keybd_event(vk, 0, packet.IsDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP, 0);
        return true;
    }

    /// <summary>
    /// Moves the cursor to an exact absolute screen position without firing
    /// any click events. Useful for hover/preview operations.
    /// </summary>
    public static void MoveCursorTo(int x, int y) => SetCursorPos(x, y);

    /// <summary>
    /// Returns the current primary screen resolution.
    /// Useful for sending to the client for coordinate scaling.
    /// </summary>
    public static Size GetScreenSize()
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
        return new Size(bounds.Width, bounds.Height);
    }
}
