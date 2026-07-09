using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TelegramLauncher.Services;

public sealed class TelegramWindowService
{
    public bool TryGetTelegramWindowHandle(out IntPtr handle)
    {
        handle = IntPtr.Zero;

        var process = Process.GetProcessesByName("Telegram")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (process is null || process.MainWindowHandle == IntPtr.Zero)
        {
            return false;
        }

        handle = process.MainWindowHandle;
        return true;
    }

    public bool TryGetTelegramWindow(out Rectangle bounds)
    {
        bounds = Rectangle.Empty;

        if (!TryGetTelegramWindowHandle(out var handle))
        {
            return false;
        }

        if (!GetWindowRect(handle, out var rect))
        {
            return false;
        }

        bounds = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        return true;
    }

    public bool IsTelegramForeground()
    {
        if (!TryGetTelegramWindowHandle(out var telegramHandle))
        {
            return false;
        }

        return GetForegroundWindow() == telegramHandle;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
